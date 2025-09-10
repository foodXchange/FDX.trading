using System.Text.Json;
using FoodX.EmailService.Models;
using FoodX.EmailService.Models.DTOs;
using FoodX.EmailService.Data;
using FoodX.EmailService.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.SignalR;

namespace FoodX.EmailService.Services;

public class EmailReceivingService : IEmailReceivingService
{
    private readonly EmailDbContext _context;
    private readonly ILogger<EmailReceivingService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IHubContext<EmailHub> _hubContext;

    public EmailReceivingService(
        EmailDbContext context,
        ILogger<EmailReceivingService> logger,
        IConfiguration configuration,
        IMemoryCache cache,
        IBlobStorageService blobStorageService,
        IHubContext<EmailHub> hubContext)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _cache = cache;
        _blobStorageService = blobStorageService;
        _hubContext = hubContext;
    }

    public async Task<Email> ProcessInboundEmailAsync(SendGridInboundEmail inboundEmail)
    {
        try
        {
            _logger.LogInformation($"Processing inbound email from {inboundEmail.From} to {inboundEmail.To}");

            // Extract clean email addresses
            var fromEmail = ExtractEmailAddress(inboundEmail.From);
            var toEmail = ExtractEmailAddress(inboundEmail.To);

            // Check if this is part of an existing thread
            EmailThread? thread = null;
            var existingEmails = await _context.Emails
                .Where(e => (e.FromEmail == fromEmail && e.ToEmail == toEmail) ||
                           (e.FromEmail == toEmail && e.ToEmail == fromEmail))
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync();

            if (existingEmails?.ThreadId != null)
            {
                thread = await _context.EmailThreads.FindAsync(existingEmails.ThreadId);
            }
            else if (!string.IsNullOrEmpty(inboundEmail.Subject))
            {
                // Try to find thread by subject
                var cleanSubject = inboundEmail.Subject.Replace("RE:", "", StringComparison.OrdinalIgnoreCase)
                                                        .Replace("FW:", "", StringComparison.OrdinalIgnoreCase)
                                                        .Trim();
                
                thread = await _context.EmailThreads
                    .FirstOrDefaultAsync(t => t.Subject.Contains(cleanSubject));
            }

            // Create thread if it doesn't exist
            if (thread == null)
            {
                thread = new EmailThread
                {
                    Subject = inboundEmail.Subject,
                    ParticipantEmails = JsonSerializer.Serialize(new[] { fromEmail, toEmail }),
                    LastActivityAt = DateTime.UtcNow,
                    EmailCount = 0,
                    HasUnread = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.EmailThreads.Add(thread);
                await _context.SaveChangesAsync();
            }

            // Create email record
            var email = new Email
            {
                FromEmail = fromEmail,
                ToEmail = toEmail,
                Subject = inboundEmail.Subject,
                HtmlBody = inboundEmail.Html,
                PlainTextBody = inboundEmail.Text,
                Direction = "Inbound",
                Status = "Received",
                Provider = "SendGrid",
                ThreadId = thread.Id,
                ReceivedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            // Parse headers for additional metadata
            if (!string.IsNullOrEmpty(inboundEmail.Headers))
            {
                try
                {
                    var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(inboundEmail.Headers);
                    if (headers != null)
                    {
                        if (headers.TryGetValue("Message-ID", out var messageId))
                            email.MessageId = messageId;
                        
                        // Store custom headers in metadata
                        email.MetaData = JsonSerializer.Serialize(new
                        {
                            Headers = headers,
                            SpamScore = inboundEmail.SpamScore,
                            Dkim = inboundEmail.Dkim,
                            Spf = inboundEmail.Spf
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse email headers");
                }
            }

            // Try to match with supplier/buyer based on email domain
            await MatchEmailToEntity(email, fromEmail);

            _context.Emails.Add(email);
            await _context.SaveChangesAsync();

            // Process attachments if any - upload to blob storage
            if (inboundEmail.AttachmentCount > 0 && inboundEmail.Attachments != null)
            {
                foreach (var attachment in inboundEmail.Attachments)
                {
                    try
                    {
                        using var memoryStream = new MemoryStream();
                        await attachment.Value.CopyToAsync(memoryStream);
                        var attachmentData = memoryStream.ToArray();

                        // Upload to blob storage
                        var contentType = GetContentTypeFromFileName(attachment.Key);
                        var blobUrl = await _blobStorageService.UploadAttachmentAsync(
                            attachmentData, 
                            attachment.Key, 
                            contentType);

                        var emailAttachment = new EmailAttachment
                        {
                            EmailId = email.Id,
                            FileName = attachment.Key,
                            BlobUrl = blobUrl, // Store blob URL instead of content
                            FileSize = attachmentData.Length,
                            ContentType = contentType,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.EmailAttachments.Add(emailAttachment);
                        _logger.LogInformation($"Uploaded attachment {attachment.Key} to blob storage");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to upload attachment {attachment.Key} to blob storage");
                        // Continue processing other attachments
                    }
                }
                await _context.SaveChangesAsync();
            }

            // Update thread
            thread.LastActivityAt = DateTime.UtcNow;
            thread.EmailCount++;
            thread.HasUnread = true;
            await _context.SaveChangesAsync();

            // Send real-time notification to connected clients
            try
            {
                await _hubContext.Clients.All.SendAsync("NewEmail", new
                {
                    EmailId = email.Id,
                    FromEmail = email.FromEmail,
                    ToEmail = email.ToEmail,
                    Subject = email.Subject,
                    ReceivedAt = email.ReceivedAt,
                    ThreadId = email.ThreadId,
                    HasAttachments = inboundEmail.AttachmentCount > 0
                });

                // Send specific notification to the recipient
                await _hubContext.Clients.Group($"user_{email.ToEmail}").SendAsync("EmailReceived", new
                {
                    EmailId = email.Id,
                    FromEmail = email.FromEmail,
                    Subject = email.Subject,
                    Preview = !string.IsNullOrEmpty(email.PlainTextBody) ? 
                        email.PlainTextBody.Substring(0, Math.Min(150, email.PlainTextBody.Length)) : "",
                    ReceivedAt = email.ReceivedAt,
                    ThreadId = email.ThreadId
                });

                _logger.LogDebug($"Sent SignalR notifications for new email {email.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to send SignalR notification for email {email.Id}");
            }

            _logger.LogInformation($"Successfully processed inbound email with ID {email.Id}");
            return email;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing inbound email");
            throw;
        }
    }

    public async Task<Email> ProcessAzureEmailEventAsync(AzureEmailEvent emailEvent)
    {
        try
        {
            _logger.LogInformation($"Processing Azure email event: {emailEvent.EventType}");

            if (emailEvent.Data == null)
                throw new ArgumentException("Email event data is null");

            // Find existing email by message ID
            var email = await _context.Emails
                .FirstOrDefaultAsync(e => e.MessageId == emailEvent.Data.MessageId);

            if (email == null)
            {
                // Create new email record if not found
                email = new Email
                {
                    MessageId = emailEvent.Data.MessageId,
                    FromEmail = emailEvent.Data.From,
                    ToEmail = emailEvent.Data.To,
                    Subject = emailEvent.Subject,
                    Direction = "Inbound",
                    Status = emailEvent.Data.Status,
                    Provider = "AzureComm",
                    ReceivedAt = emailEvent.EventTime,
                    CreatedAt = DateTime.UtcNow
                };

                await MatchEmailToEntity(email, emailEvent.Data.From);
                _context.Emails.Add(email);
            }
            else
            {
                // Update existing email status
                email.Status = emailEvent.Data.Status;
                
                if (emailEvent.Data.Status == "Delivered")
                {
                    email.SentAt = emailEvent.EventTime;
                }
                else if (emailEvent.Data.Status == "Failed")
                {
                    email.FailedAt = emailEvent.EventTime;
                    email.ErrorMessage = emailEvent.Data.DeliveryStatusDetails;
                }
            }

            await _context.SaveChangesAsync();

            // Send real-time notification about email status update
            try
            {
                await _hubContext.Clients.All.SendAsync("EmailStatusUpdate", new
                {
                    EmailId = email.Id,
                    MessageId = email.MessageId,
                    Status = email.Status,
                    FromEmail = email.FromEmail,
                    ToEmail = email.ToEmail,
                    Subject = email.Subject,
                    UpdatedAt = DateTime.UtcNow
                });

                // Send specific notification to the sender about status
                await _hubContext.Clients.Group($"user_{email.FromEmail}").SendAsync("EmailDeliveryUpdate", new
                {
                    EmailId = email.Id,
                    Status = email.Status,
                    Subject = email.Subject,
                    ToEmail = email.ToEmail,
                    UpdatedAt = DateTime.UtcNow,
                    ErrorMessage = email.ErrorMessage
                });

                _logger.LogDebug($"Sent SignalR notifications for email status update {email.Id} - {email.Status}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to send SignalR notification for email status update {email.Id}");
            }

            return email;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Azure email event");
            throw;
        }
    }

    public async Task MarkEmailAsReadAsync(int emailId)
    {
        var email = await _context.Emails.FindAsync(emailId);
        if (email != null && email.ReadAt == null)
        {
            email.ReadAt = DateTime.UtcNow;
            
            // Update thread unread status
            if (email.ThreadId.HasValue)
            {
                var thread = await _context.EmailThreads
                    .Include(t => t.Emails)
                    .FirstOrDefaultAsync(t => t.Id == email.ThreadId.Value);
                
                if (thread != null)
                {
                    thread.HasUnread = thread.Emails.Any(e => e.ReadAt == null && e.Direction == "Inbound");
                }
            }
            
            await _context.SaveChangesAsync();

            // Send real-time notification about email read status
            try
            {
                await _hubContext.Clients.Group($"user_{email.ToEmail}").SendAsync("EmailRead", new
                {
                    EmailId = email.Id,
                    ThreadId = email.ThreadId,
                    ReadAt = email.ReadAt
                });

                _logger.LogDebug($"Sent SignalR notification for email read {email.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to send SignalR notification for email read {email.Id}");
            }
        }
    }

    public async Task<List<Email>> GetInboxAsync(string userEmail, int page = 1, int pageSize = 20, string? folder = null, string? category = null, string? search = null)
    {
        // Create cache key based on parameters
        var cacheKey = $"inbox_{userEmail}_{page}_{pageSize}_{folder}_{category}_{search}";
        
        // Try to get from cache first
        if (_cache.TryGetValue<List<Email>>(cacheKey, out var cachedEmails))
        {
            _logger.LogDebug($"Returning cached emails for key: {cacheKey}");
            return cachedEmails!;
        }

        var query = _context.Emails
            .Include(e => e.Attachments)
            .Include(e => e.Thread)
            .AsQueryable();

        // Base user filter - don't show permanently deleted emails unless in deleted folder
        if (folder == "deleted")
        {
            query = query.Where(e => (e.ToEmail == userEmail || e.FromEmail == userEmail) && e.IsDeleted);
        }
        else
        {
            query = query.Where(e => (e.ToEmail == userEmail || e.FromEmail == userEmail) && !e.IsDeleted);
        }

        // Folder filtering
        if (!string.IsNullOrEmpty(folder))
        {
            switch (folder.ToLower())
            {
                case "inbox":
                    query = query.Where(e => e.Folder == "inbox" && e.Direction == "Inbound" && !e.IsArchived);
                    break;
                case "sent":
                    query = query.Where(e => e.Folder == "sent" || e.Direction == "Outbound");
                    break;
                case "drafts":
                    query = query.Where(e => e.Folder == "drafts" || e.Status == "Draft");
                    break;
                case "archived":
                    query = query.Where(e => e.IsArchived);
                    break;
                case "deleted":
                    // Already filtered above
                    break;
            }
        }
        else
        {
            // Default to inbox view
            query = query.Where(e => e.Folder == "inbox" && e.Direction == "Inbound" && !e.IsArchived);
        }

        // Category filtering
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(e => e.Category == category);
        }

        // Optimized search filtering
        if (!string.IsNullOrEmpty(search))
        {
            query = ApplySearchFilter(query, search);
        }

        var emails = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Cache the results for 2 minutes
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(2))
            .SetSlidingExpiration(TimeSpan.FromMinutes(1));
        
        _cache.Set(cacheKey, emails, cacheOptions);
        _logger.LogDebug($"Cached {emails.Count} emails for key: {cacheKey}");

        return emails;
    }

    public async Task<EmailThread> GetThreadAsync(int threadId)
    {
        // Cache thread data
        var cacheKey = $"thread_{threadId}";
        
        if (_cache.TryGetValue<EmailThread>(cacheKey, out var cachedThread))
        {
            _logger.LogDebug($"Returning cached thread for ID: {threadId}");
            return cachedThread!;
        }

        var thread = await _context.EmailThreads
            .Include(t => t.Emails)
                .ThenInclude(e => e.Attachments)
            .FirstOrDefaultAsync(t => t.Id == threadId);

        if (thread == null)
            throw new ArgumentException($"Thread with ID {threadId} not found");

        // Cache for 5 minutes
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
        
        _cache.Set(cacheKey, thread, cacheOptions);

        return thread;
    }

    private string ExtractEmailAddress(string emailString)
    {
        // Extract email from strings like "John Doe <john@example.com>"
        if (emailString.Contains('<') && emailString.Contains('>'))
        {
            var start = emailString.IndexOf('<') + 1;
            var end = emailString.IndexOf('>');
            return emailString.Substring(start, end - start).Trim();
        }
        return emailString.Trim();
    }

    private Task MatchEmailToEntity(Email email, string fromEmail)
    {
        // Try to match email domain with existing suppliers/buyers
        var domain = fromEmail.Split('@').LastOrDefault()?.ToLower();
        if (string.IsNullOrEmpty(domain))
            return Task.CompletedTask;

        // This would normally query your FoodX database to match suppliers/buyers
        // For now, we'll just log this as a placeholder
        _logger.LogInformation($"Attempting to match email from domain {domain} to supplier/buyer");
        
        // TODO: Implement actual matching logic with FoodX database
        // Example:
        // var supplier = await _foodXContext.Suppliers
        //     .FirstOrDefaultAsync(s => s.CompanyEmail.EndsWith(domain));
        // if (supplier != null)
        // {
        //     email.SupplierId = supplier.Id;
        //     email.Category = "Supplier";
        // }
        
        return Task.CompletedTask;
    }

    public async Task DeleteEmailAsync(int emailId)
    {
        var email = await _context.Emails.FirstOrDefaultAsync(e => e.Id == emailId);
        
        if (email == null)
        {
            throw new ArgumentException($"Email with ID {emailId} not found");
        }

        // Soft delete - move to deleted folder
        email.IsDeleted = true;
        email.DeletedAt = DateTime.UtcNow;
        email.Folder = "deleted";
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation($"Email {emailId} moved to deleted folder (soft delete)");
    }

    public async Task ArchiveEmailAsync(int emailId)
    {
        var email = await _context.Emails.FirstOrDefaultAsync(e => e.Id == emailId);
        
        if (email == null)
        {
            throw new ArgumentException($"Email with ID {emailId} not found");
        }

        email.IsArchived = true;
        email.ArchivedAt = DateTime.UtcNow;
        email.Folder = "archived";
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation($"Email {emailId} archived successfully");
    }

    public async Task RestoreEmailAsync(int emailId)
    {
        var email = await _context.Emails.FirstOrDefaultAsync(e => e.Id == emailId);
        
        if (email == null)
        {
            throw new ArgumentException($"Email with ID {emailId} not found");
        }

        email.IsDeleted = false;
        email.DeletedAt = null;
        email.IsArchived = false;
        email.ArchivedAt = null;
        
        // Restore to appropriate folder based on direction
        email.Folder = email.Direction == "Inbound" ? "inbox" : "sent";
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation($"Email {emailId} restored successfully");
    }

    public async Task PermanentlyDeleteEmailAsync(int emailId)
    {
        var email = await _context.Emails
            .Include(e => e.Attachments)
            .FirstOrDefaultAsync(e => e.Id == emailId);
        
        if (email == null)
        {
            throw new ArgumentException($"Email with ID {emailId} not found");
        }

        // Delete attachments from storage if any
        if (email.Attachments?.Any() == true)
        {
            foreach (var attachment in email.Attachments)
            {
                // TODO: Delete from Azure Storage if implemented
                _logger.LogInformation($"Permanently deleting attachment {attachment.FileName} for email {emailId}");
            }
        }

        // Check if this is the last email in a thread
        if (email.ThreadId.HasValue)
        {
            var thread = await _context.EmailThreads
                .Include(t => t.Emails)
                .FirstOrDefaultAsync(t => t.Id == email.ThreadId.Value);
            
            if (thread != null && thread.Emails.Count == 1)
            {
                // Delete the thread if this is the last email
                _context.EmailThreads.Remove(thread);
            }
            else if (thread != null)
            {
                // Update thread metadata
                thread.EmailCount--;
                var remainingEmails = thread.Emails.Where(e => e.Id != emailId);
                if (remainingEmails.Any())
                {
                    thread.LastActivityAt = remainingEmails.Max(e => e.CreatedAt);
                }
            }
        }

        _context.Emails.Remove(email);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation($"Email {emailId} permanently deleted successfully");
    }

    /// <summary>
    /// Optimized search filter with smart query planning
    /// </summary>
    private IQueryable<Email> ApplySearchFilter(IQueryable<Email> query, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var trimmedSearch = searchTerm.Trim();
        
        // For email addresses, search more efficiently
        if (trimmedSearch.Contains("@"))
        {
            return query.Where(e => 
                e.FromEmail.Contains(trimmedSearch) || 
                e.ToEmail.Contains(trimmedSearch)
            );
        }

        // For single words, optimize query order (most selective first)
        if (!trimmedSearch.Contains(" "))
        {
            return query.Where(e => 
                e.Subject.Contains(trimmedSearch) || 
                e.FromEmail.Contains(trimmedSearch) ||
                e.ToEmail.Contains(trimmedSearch) ||
                (e.PlainTextBody != null && e.PlainTextBody.Contains(trimmedSearch)) ||
                (e.HtmlBody != null && e.HtmlBody.Contains(trimmedSearch))
            );
        }

        // For multi-word searches, split and search efficiently
        var searchWords = trimmedSearch.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (searchWords.Length == 1)
        {
            var word = searchWords[0];
            return query.Where(e => 
                e.Subject.Contains(word) || 
                e.FromEmail.Contains(word) ||
                e.ToEmail.Contains(word) ||
                (e.PlainTextBody != null && e.PlainTextBody.Contains(word)) ||
                (e.HtmlBody != null && e.HtmlBody.Contains(word))
            );
        }

        // For multiple words, require all words to be present (AND logic)
        foreach (var word in searchWords.Take(3)) // Limit to first 3 words for performance
        {
            var currentWord = word;
            query = query.Where(e => 
                e.Subject.Contains(currentWord) || 
                e.FromEmail.Contains(currentWord) ||
                e.ToEmail.Contains(currentWord) ||
                (e.PlainTextBody != null && e.PlainTextBody.Contains(currentWord)) ||
                (e.HtmlBody != null && e.HtmlBody.Contains(currentWord))
            );
        }

        return query;
    }

    /// <summary>
    /// Advanced search with multiple filters and sorting options
    /// </summary>
    public async Task<List<Email>> AdvancedSearchAsync(
        string userEmail,
        string? searchTerm = null,
        string? fromEmail = null,
        string? toEmail = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        bool? hasAttachments = null,
        bool? unreadOnly = null,
        string? category = null,
        string sortBy = "date",
        bool sortDescending = true,
        int page = 1,
        int pageSize = 20)
    {
        var cacheKey = $"search_{userEmail}_{searchTerm}_{fromEmail}_{toEmail}_{dateFrom}_{dateTo}_{hasAttachments}_{unreadOnly}_{category}_{sortBy}_{sortDescending}_{page}_{pageSize}";
        
        if (_cache.TryGetValue<List<Email>>(cacheKey, out var cachedResults))
        {
            return cachedResults!;
        }

        var query = _context.Emails
            .Include(e => e.Attachments)
            .Include(e => e.Thread)
            .Where(e => (e.ToEmail == userEmail || e.FromEmail == userEmail) && !e.IsDeleted);

        // Apply filters
        if (!string.IsNullOrEmpty(searchTerm))
            query = ApplySearchFilter(query, searchTerm);

        if (!string.IsNullOrEmpty(fromEmail))
            query = query.Where(e => e.FromEmail.Contains(fromEmail));

        if (!string.IsNullOrEmpty(toEmail))
            query = query.Where(e => e.ToEmail.Contains(toEmail));

        if (dateFrom.HasValue)
            query = query.Where(e => e.CreatedAt >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(e => e.CreatedAt <= dateTo.Value);

        if (hasAttachments.HasValue)
            query = hasAttachments.Value 
                ? query.Where(e => e.Attachments != null && e.Attachments.Any())
                : query.Where(e => e.Attachments == null || !e.Attachments.Any());

        if (unreadOnly == true)
            query = query.Where(e => e.ReadAt == null && e.Direction == "Inbound");

        if (!string.IsNullOrEmpty(category))
            query = query.Where(e => e.Category == category);

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "subject" => sortDescending 
                ? query.OrderByDescending(e => e.Subject)
                : query.OrderBy(e => e.Subject),
            "from" => sortDescending
                ? query.OrderByDescending(e => e.FromEmail)
                : query.OrderBy(e => e.FromEmail),
            "size" => sortDescending
                ? query.OrderByDescending(e => (e.HtmlBody ?? "").Length + (e.PlainTextBody ?? "").Length)
                : query.OrderBy(e => (e.HtmlBody ?? "").Length + (e.PlainTextBody ?? "").Length),
            _ => sortDescending
                ? query.OrderByDescending(e => e.CreatedAt)
                : query.OrderBy(e => e.CreatedAt)
        };

        var results = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Cache results for 1 minute
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
        
        _cache.Set(cacheKey, results, cacheOptions);

        return results;
    }

    /// <summary>
    /// Get search suggestions based on user's email history
    /// </summary>
    public async Task<List<string>> GetSearchSuggestionsAsync(string userEmail, string searchTerm, int maxSuggestions = 5)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
            return new List<string>();

        var cacheKey = $"suggestions_{userEmail}_{searchTerm}_{maxSuggestions}";
        
        if (_cache.TryGetValue<List<string>>(cacheKey, out var cachedSuggestions))
        {
            return cachedSuggestions!;
        }

        var suggestions = new HashSet<string>();

        // Get subject suggestions
        var subjectSuggestions = await _context.Emails
            .Where(e => (e.ToEmail == userEmail || e.FromEmail == userEmail) && 
                       !e.IsDeleted &&
                       e.Subject.Contains(searchTerm))
            .Select(e => e.Subject)
            .Distinct()
            .Take(maxSuggestions)
            .ToListAsync();

        foreach (var suggestion in subjectSuggestions)
            suggestions.Add(suggestion);

        // Get email address suggestions
        var emailSuggestions = await _context.Emails
            .Where(e => (e.ToEmail == userEmail || e.FromEmail == userEmail) && 
                       !e.IsDeleted &&
                       (e.FromEmail.Contains(searchTerm) || e.ToEmail.Contains(searchTerm)))
            .SelectMany(e => new[] { e.FromEmail, e.ToEmail })
            .Where(email => email.Contains(searchTerm) && email != userEmail)
            .Distinct()
            .Take(maxSuggestions)
            .ToListAsync();

        foreach (var suggestion in emailSuggestions)
            suggestions.Add(suggestion);

        var result = suggestions.Take(maxSuggestions).ToList();

        // Cache for 5 minutes
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
        
        _cache.Set(cacheKey, result, cacheOptions);

        return result;
    }

    /// <summary>
    /// Helper method to determine content type from file extension
    /// </summary>
    private string GetContentTypeFromFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            _ => "application/octet-stream"
        };
    }
}
using System.Text.Json;
using FoodX.EmailService.Models;
using FoodX.EmailService.Models.DTOs;
using FoodX.EmailService.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodX.EmailService.Services;

public class EmailReceivingService : IEmailReceivingService
{
    private readonly EmailDbContext _context;
    private readonly ILogger<EmailReceivingService> _logger;
    private readonly IConfiguration _configuration;

    public EmailReceivingService(
        EmailDbContext context,
        ILogger<EmailReceivingService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
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

            // Process attachments if any
            if (inboundEmail.AttachmentCount > 0 && inboundEmail.Attachments != null)
            {
                foreach (var attachment in inboundEmail.Attachments)
                {
                    using var memoryStream = new MemoryStream();
                    await attachment.Value.CopyToAsync(memoryStream);
                    
                    var emailAttachment = new EmailAttachment
                    {
                        EmailId = email.Id,
                        FileName = attachment.Key,
                        Content = memoryStream.ToArray(),
                        FileSize = memoryStream.Length,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.EmailAttachments.Add(emailAttachment);
                }
                await _context.SaveChangesAsync();
            }

            // Update thread
            thread.LastActivityAt = DateTime.UtcNow;
            thread.EmailCount++;
            thread.HasUnread = true;
            await _context.SaveChangesAsync();

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
        }
    }

    public async Task<List<Email>> GetInboxAsync(string userEmail, int page = 1, int pageSize = 20)
    {
        var query = _context.Emails
            .Include(e => e.Attachments)
            .Include(e => e.Thread)
            .Where(e => e.ToEmail == userEmail || e.FromEmail == userEmail)
            .OrderByDescending(e => e.CreatedAt);

        var emails = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return emails;
    }

    public async Task<EmailThread> GetThreadAsync(int threadId)
    {
        var thread = await _context.EmailThreads
            .Include(t => t.Emails)
                .ThenInclude(e => e.Attachments)
            .FirstOrDefaultAsync(t => t.Id == threadId);

        if (thread == null)
            throw new ArgumentException($"Thread with ID {threadId} not found");

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
}
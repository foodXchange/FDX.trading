using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text.Json;
using FoodX.EmailService.Models;
using FoodX.EmailService.Models.DTOs;
using FoodX.EmailService.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodX.EmailService.Services;

public class EmailSendingService : IEmailSendingService
{
    private readonly ISendGridClient _sendGridClient;
    private readonly EmailDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSendingService> _logger;
    private readonly string _defaultFromEmail;
    private readonly string _defaultFromName;

    public EmailSendingService(
        ISendGridClient sendGridClient,
        EmailDbContext context,
        IConfiguration configuration,
        ILogger<EmailSendingService> logger)
    {
        _sendGridClient = sendGridClient;
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _defaultFromEmail = _configuration["SendGrid:FromEmail"] ?? "noreply@fdx.trading";
        _defaultFromName = _configuration["SendGrid:FromName"] ?? "FoodX Platform";
    }

    public async Task<Email> SendEmailAsync(EmailRequest request)
    {
        try
        {
            // Create email record
            var email = new Email
            {
                FromEmail = request.From ?? _defaultFromEmail,
                ToEmail = request.To,
                CcEmail = request.Cc != null ? string.Join(";", request.Cc) : null,
                BccEmail = request.Bcc != null ? string.Join(";", request.Bcc) : null,
                Subject = request.Subject,
                HtmlBody = request.HtmlBody,
                PlainTextBody = request.PlainTextBody,
                Direction = "Outbound",
                Status = "Pending",
                Provider = "SendGrid",
                Category = request.Category,
                SupplierId = request.SupplierId,
                BuyerId = request.BuyerId,
                UserId = request.UserId,
                MetaData = request.MetaData != null ? JsonSerializer.Serialize(request.MetaData) : null,
                CreatedAt = DateTime.UtcNow
            };

            // Handle thread
            if (request.ReplyToEmailId.HasValue)
            {
                var originalEmail = await _context.Emails.FindAsync(request.ReplyToEmailId.Value);
                if (originalEmail?.ThreadId != null)
                {
                    email.ThreadId = originalEmail.ThreadId;
                }
            }

            // Save email to database
            _context.Emails.Add(email);
            await _context.SaveChangesAsync();

            // Handle attachments
            if (request.Attachments != null && request.Attachments.Any())
            {
                foreach (var att in request.Attachments)
                {
                    var attachment = new EmailAttachment
                    {
                        EmailId = email.Id,
                        FileName = att.FileName,
                        ContentType = att.ContentType,
                        Content = Convert.FromBase64String(att.Base64Content),
                        FileSize = Convert.FromBase64String(att.Base64Content).Length,
                        IsInline = att.IsInline,
                        ContentId = att.ContentId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.EmailAttachments.Add(attachment);
                }
                await _context.SaveChangesAsync();
            }

            // Send via SendGrid
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(email.FromEmail, _defaultFromName),
                Subject = email.Subject,
                PlainTextContent = email.PlainTextBody,
                HtmlContent = email.HtmlBody
            };

            msg.AddTo(new EmailAddress(email.ToEmail));

            // Add CC recipients
            if (!string.IsNullOrEmpty(email.CcEmail))
            {
                foreach (var cc in email.CcEmail.Split(';'))
                {
                    if (!string.IsNullOrWhiteSpace(cc))
                        msg.AddCc(new EmailAddress(cc.Trim()));
                }
            }

            // Add BCC recipients
            if (!string.IsNullOrEmpty(email.BccEmail))
            {
                foreach (var bcc in email.BccEmail.Split(';'))
                {
                    if (!string.IsNullOrWhiteSpace(bcc))
                        msg.AddBcc(new EmailAddress(bcc.Trim()));
                }
            }

            // Add attachments to SendGrid message
            if (request.Attachments != null)
            {
                foreach (var att in request.Attachments)
                {
                    msg.AddAttachment(att.FileName, att.Base64Content, att.ContentType);
                }
            }

            // Set custom headers for tracking
            msg.AddHeader("X-FoodX-EmailId", email.Id.ToString());
            if (email.Category != null)
                msg.AddHeader("X-FoodX-Category", email.Category);

            // Send email
            var response = await _sendGridClient.SendEmailAsync(msg);

            // Update email status
            if (response.StatusCode == System.Net.HttpStatusCode.Accepted || 
                response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                email.Status = "Sent";
                email.SentAt = DateTime.UtcNow;
                
                // Get message ID from response headers if available
                if (response.Headers != null && response.Headers.TryGetValues("X-Message-Id", out var messageIds))
                {
                    email.MessageId = messageIds.FirstOrDefault();
                }

                _logger.LogInformation($"Email sent successfully to {email.ToEmail} with ID {email.Id}");
            }
            else
            {
                email.Status = "Failed";
                email.FailedAt = DateTime.UtcNow;
                var body = await response.Body.ReadAsStringAsync();
                email.ErrorMessage = $"SendGrid Error: {response.StatusCode} - {body}";
                _logger.LogError($"Failed to send email to {email.ToEmail}: {email.ErrorMessage}");
            }

            // Update thread if applicable
            if (email.ThreadId.HasValue)
            {
                var thread = await _context.EmailThreads.FindAsync(email.ThreadId.Value);
                if (thread != null)
                {
                    thread.LastActivityAt = DateTime.UtcNow;
                    thread.EmailCount++;
                }
            }

            await _context.SaveChangesAsync();
            return email;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending email to {request.To}");
            throw;
        }
    }

    public async Task<Email> SendTemplateEmailAsync(string templateId, EmailRequest request)
    {
        // For now, just use regular send. Later we can implement SendGrid template support
        return await SendEmailAsync(request);
    }

    public async Task<bool> ResendEmailAsync(int emailId)
    {
        var originalEmail = await _context.Emails
            .Include(e => e.Attachments)
            .FirstOrDefaultAsync(e => e.Id == emailId);

        if (originalEmail == null)
            return false;

        var request = new EmailRequest
        {
            To = originalEmail.ToEmail,
            From = originalEmail.FromEmail,
            Cc = originalEmail.CcEmail?.Split(';').ToList(),
            Bcc = originalEmail.BccEmail?.Split(';').ToList(),
            Subject = originalEmail.Subject,
            HtmlBody = originalEmail.HtmlBody,
            PlainTextBody = originalEmail.PlainTextBody,
            Category = originalEmail.Category,
            SupplierId = originalEmail.SupplierId,
            BuyerId = originalEmail.BuyerId,
            UserId = originalEmail.UserId
        };

        // Add attachments if any
        if (originalEmail.Attachments.Any())
        {
            request.Attachments = originalEmail.Attachments.Select(a => new AttachmentDto
            {
                FileName = a.FileName,
                ContentType = a.ContentType ?? "application/octet-stream",
                Base64Content = Convert.ToBase64String(a.Content ?? Array.Empty<byte>()),
                IsInline = a.IsInline,
                ContentId = a.ContentId
            }).ToList();
        }

        var newEmail = await SendEmailAsync(request);
        return newEmail.Status == "Sent";
    }

    public async Task<List<Email>> SendBulkEmailsAsync(List<EmailRequest> requests)
    {
        var emails = new List<Email>();
        
        // Process in batches to avoid overwhelming the API
        const int batchSize = 10;
        for (int i = 0; i < requests.Count; i += batchSize)
        {
            var batch = requests.Skip(i).Take(batchSize);
            var tasks = batch.Select(SendEmailAsync).ToArray();
            var results = await Task.WhenAll(tasks);
            emails.AddRange(results);
            
            // Small delay between batches
            if (i + batchSize < requests.Count)
                await Task.Delay(1000);
        }

        return emails;
    }

    public async Task<Email> SendReplyAsync(int originalEmailId, EmailRequest replyRequest)
    {
        var originalEmail = await _context.Emails
            .Include(e => e.Thread)
            .FirstOrDefaultAsync(e => e.Id == originalEmailId);

        if (originalEmail == null)
            throw new ArgumentException($"Original email with ID {originalEmailId} not found");

        // Ensure reply is part of the same thread
        if (originalEmail.ThreadId == null)
        {
            // Create a new thread for this conversation
            var thread = new EmailThread
            {
                Subject = originalEmail.Subject,
                ParticipantEmails = JsonSerializer.Serialize(new[] { originalEmail.FromEmail, originalEmail.ToEmail }),
                LastActivityAt = DateTime.UtcNow,
                EmailCount = 1,
                Category = originalEmail.Category,
                SupplierId = originalEmail.SupplierId,
                BuyerId = originalEmail.BuyerId,
                UserId = originalEmail.UserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmailThreads.Add(thread);
            await _context.SaveChangesAsync();

            originalEmail.ThreadId = thread.Id;
            await _context.SaveChangesAsync();
        }

        // Set thread ID for reply
        replyRequest.ReplyToEmailId = originalEmailId;
        
        // Ensure subject has RE: prefix
        if (!replyRequest.Subject.StartsWith("RE:", StringComparison.OrdinalIgnoreCase))
        {
            replyRequest.Subject = $"RE: {originalEmail.Subject}";
        }

        // Swap to/from for reply
        if (string.IsNullOrEmpty(replyRequest.From))
        {
            replyRequest.From = originalEmail.ToEmail;
        }
        if (string.IsNullOrEmpty(replyRequest.To))
        {
            replyRequest.To = originalEmail.FromEmail;
        }

        return await SendEmailAsync(replyRequest);
    }
}
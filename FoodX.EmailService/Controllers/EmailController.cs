using Microsoft.AspNetCore.Mvc;
using FoodX.EmailService.Models.DTOs;
using FoodX.EmailService.Services;

namespace FoodX.EmailService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IEmailSendingService _emailSendingService;
    private readonly IEmailReceivingService _emailReceivingService;
    private readonly ILogger<EmailController> _logger;

    public EmailController(
        IEmailSendingService emailSendingService,
        IEmailReceivingService emailReceivingService,
        ILogger<EmailController> logger)
    {
        _emailSendingService = emailSendingService;
        _emailReceivingService = emailReceivingService;
        _logger = logger;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = await _emailSendingService.SendEmailAsync(request);
            
            return Ok(new
            {
                Success = email.Status == "Sent",
                EmailId = email.Id,
                Status = email.Status,
                SentAt = email.SentAt,
                Message = email.Status == "Sent" ? "Email sent successfully" : "Email queued for sending"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email");
            return StatusCode(500, new { Error = "Failed to send email", Details = ex.Message });
        }
    }

    [HttpPost("send-bulk")]
    public async Task<IActionResult> SendBulkEmails([FromBody] List<EmailRequest> requests)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var emails = await _emailSendingService.SendBulkEmailsAsync(requests);
            
            return Ok(new
            {
                Success = true,
                TotalSent = emails.Count(e => e.Status == "Sent"),
                TotalFailed = emails.Count(e => e.Status == "Failed"),
                Emails = emails.Select(e => new
                {
                    e.Id,
                    e.ToEmail,
                    e.Status,
                    e.SentAt
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk emails");
            return StatusCode(500, new { Error = "Failed to send bulk emails", Details = ex.Message });
        }
    }

    [HttpPost("reply/{emailId}")]
    public async Task<IActionResult> SendReply(int emailId, [FromBody] EmailRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = await _emailSendingService.SendReplyAsync(emailId, request);
            
            return Ok(new
            {
                Success = email.Status == "Sent",
                EmailId = email.Id,
                ThreadId = email.ThreadId,
                Status = email.Status,
                SentAt = email.SentAt
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending reply to email {emailId}");
            return StatusCode(500, new { Error = "Failed to send reply", Details = ex.Message });
        }
    }

    [HttpPost("resend/{emailId}")]
    public async Task<IActionResult> ResendEmail(int emailId)
    {
        try
        {
            var success = await _emailSendingService.ResendEmailAsync(emailId);
            
            if (success)
                return Ok(new { Success = true, Message = "Email resent successfully" });
            else
                return NotFound(new { Success = false, Message = "Email not found or could not be resent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error resending email {emailId}");
            return StatusCode(500, new { Error = "Failed to resend email", Details = ex.Message });
        }
    }

    [HttpGet("inbox")]
    public async Task<IActionResult> GetInbox([FromQuery] string userEmail, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (string.IsNullOrEmpty(userEmail))
                return BadRequest(new { Error = "User email is required" });

            var emails = await _emailReceivingService.GetInboxAsync(userEmail, page, pageSize);
            
            var response = emails.Select(e => new EmailResponse
            {
                Id = e.Id,
                MessageId = e.MessageId,
                ThreadId = e.ThreadId,
                From = e.FromEmail,
                To = e.ToEmail,
                Cc = e.CcEmail,
                Subject = e.Subject,
                HtmlBody = e.HtmlBody,
                PlainTextBody = e.PlainTextBody,
                Direction = e.Direction,
                Status = e.Status,
                Category = e.Category,
                CreatedAt = e.CreatedAt,
                SentAt = e.SentAt,
                ReceivedAt = e.ReceivedAt,
                ReadAt = e.ReadAt,
                Attachments = e.Attachments.Select(a => new AttachmentResponse
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    ContentType = a.ContentType,
                    FileSize = a.FileSize,
                    IsInline = a.IsInline,
                    DownloadUrl = $"/api/email/attachment/{a.Id}"
                }).ToList()
            });

            return Ok(new
            {
                Success = true,
                Page = page,
                PageSize = pageSize,
                Emails = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inbox");
            return StatusCode(500, new { Error = "Failed to retrieve inbox", Details = ex.Message });
        }
    }

    [HttpGet("thread/{threadId}")]
    public async Task<IActionResult> GetThread(int threadId)
    {
        try
        {
            var thread = await _emailReceivingService.GetThreadAsync(threadId);
            
            var response = new EmailThreadResponse
            {
                Id = thread.Id,
                Subject = thread.Subject,
                Participants = System.Text.Json.JsonSerializer.Deserialize<List<string>>(thread.ParticipantEmails) ?? new List<string>(),
                LastActivityAt = thread.LastActivityAt,
                EmailCount = thread.EmailCount,
                HasUnread = thread.HasUnread,
                Category = thread.Category,
                Emails = thread.Emails.OrderBy(e => e.CreatedAt).Select(e => new EmailResponse
                {
                    Id = e.Id,
                    MessageId = e.MessageId,
                    ThreadId = e.ThreadId,
                    From = e.FromEmail,
                    To = e.ToEmail,
                    Cc = e.CcEmail,
                    Subject = e.Subject,
                    HtmlBody = e.HtmlBody,
                    PlainTextBody = e.PlainTextBody,
                    Direction = e.Direction,
                    Status = e.Status,
                    Category = e.Category,
                    CreatedAt = e.CreatedAt,
                    SentAt = e.SentAt,
                    ReceivedAt = e.ReceivedAt,
                    ReadAt = e.ReadAt,
                    Attachments = e.Attachments.Select(a => new AttachmentResponse
                    {
                        Id = a.Id,
                        FileName = a.FileName,
                        ContentType = a.ContentType,
                        FileSize = a.FileSize,
                        IsInline = a.IsInline,
                        DownloadUrl = $"/api/email/attachment/{a.Id}"
                    }).ToList()
                }).ToList()
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting thread {threadId}");
            return StatusCode(500, new { Error = "Failed to retrieve thread", Details = ex.Message });
        }
    }

    [HttpPut("mark-read/{emailId}")]
    public async Task<IActionResult> MarkAsRead(int emailId)
    {
        try
        {
            await _emailReceivingService.MarkEmailAsReadAsync(emailId);
            return Ok(new { Success = true, Message = "Email marked as read" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error marking email {emailId} as read");
            return StatusCode(500, new { Error = "Failed to mark email as read", Details = ex.Message });
        }
    }
}
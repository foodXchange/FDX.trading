using FoodX.EmailService.Models;
using FoodX.EmailService.Models.DTOs;

namespace FoodX.EmailService.Services;

public interface IEmailReceivingService
{
    Task<Email> ProcessInboundEmailAsync(SendGridInboundEmail inboundEmail);
    Task<Email> ProcessAzureEmailEventAsync(AzureEmailEvent emailEvent);
    Task MarkEmailAsReadAsync(int emailId);
    Task<List<Email>> GetInboxAsync(string userEmail, int page = 1, int pageSize = 20);
    Task<EmailThread> GetThreadAsync(int threadId);
}

public class SendGridInboundEmail
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Html { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Headers { get; set; } = string.Empty;
    public string Envelope { get; set; } = string.Empty;
    public string Charsets { get; set; } = string.Empty;
    public string SpamScore { get; set; } = string.Empty;
    public string SpamReport { get; set; } = string.Empty;
    public string Dkim { get; set; } = string.Empty;
    public string Spf { get; set; } = string.Empty;
    public int AttachmentCount { get; set; }
    public Dictionary<string, Stream>? Attachments { get; set; }
}

public class AzureEmailEvent
{
    public string Id { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public DateTime EventTime { get; set; }
    public EmailEventData? Data { get; set; }
}

public class EmailEventData
{
    public string MessageId { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? DeliveryStatusDetails { get; set; }
}
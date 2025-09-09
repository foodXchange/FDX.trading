namespace FoodX.EmailService.Models.DTOs;

public class EmailResponse
{
    public int Id { get; set; }
    public string? MessageId { get; set; }
    public int? ThreadId { get; set; }
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string? Cc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public string? PlainTextBody { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public List<AttachmentResponse>? Attachments { get; set; }
}

public class AttachmentResponse
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long FileSize { get; set; }
    public string? DownloadUrl { get; set; }
    public bool IsInline { get; set; }
}

public class EmailThreadResponse
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public List<string> Participants { get; set; } = new();
    public DateTime LastActivityAt { get; set; }
    public int EmailCount { get; set; }
    public bool HasUnread { get; set; }
    public string? Category { get; set; }
    public List<EmailResponse> Emails { get; set; } = new();
}
using System.ComponentModel.DataAnnotations;

namespace FoodX.EmailService.Models.DTOs;

public class EmailRequest
{
    [Required]
    [EmailAddress]
    public string To { get; set; } = string.Empty;

    [EmailAddress]
    public string? From { get; set; }

    public List<string>? Cc { get; set; }

    public List<string>? Bcc { get; set; }

    [Required]
    public string Subject { get; set; } = string.Empty;

    public string? HtmlBody { get; set; }

    public string? PlainTextBody { get; set; }

    public string? Category { get; set; }

    public int? SupplierId { get; set; }

    public int? BuyerId { get; set; }

    public string? UserId { get; set; }

    public Dictionary<string, string>? MetaData { get; set; }

    public List<AttachmentDto>? Attachments { get; set; }

    public int? ReplyToEmailId { get; set; }

    public bool IsHighPriority { get; set; } = false;
}

public class AttachmentDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Base64Content { get; set; } = string.Empty;
    public bool IsInline { get; set; } = false;
    public string? ContentId { get; set; }
}
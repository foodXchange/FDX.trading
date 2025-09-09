using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.EmailService.Models;

[Table("Emails")]
public class Email
{
    [Key]
    public int Id { get; set; }

    [MaxLength(255)]
    public string? MessageId { get; set; }

    public int? ThreadId { get; set; }

    [Required]
    [MaxLength(255)]
    public string FromEmail { get; set; } = string.Empty;

    [Required]
    public string ToEmail { get; set; } = string.Empty;

    public string? CcEmail { get; set; }

    public string? BccEmail { get; set; }

    [Required]
    [MaxLength(500)]
    public string Subject { get; set; } = string.Empty;

    public string? HtmlBody { get; set; }

    public string? PlainTextBody { get; set; }

    [Required]
    [MaxLength(20)]
    public string Direction { get; set; } = "Outbound"; // Inbound or Outbound

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Sent, Delivered, Failed, Received, Read

    [MaxLength(50)]
    public string? Provider { get; set; } // SendGrid, AzureComm, etc.

    public int? SupplierId { get; set; }

    public int? BuyerId { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; } // RFQ, Quote, Order, Notification, System

    public string? MetaData { get; set; } // JSON for additional data

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SentAt { get; set; }

    public DateTime? ReceivedAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime? FailedAt { get; set; }

    public string? ErrorMessage { get; set; }

    // Navigation properties
    [ForeignKey("ThreadId")]
    public virtual EmailThread? Thread { get; set; }

    public virtual ICollection<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();
}
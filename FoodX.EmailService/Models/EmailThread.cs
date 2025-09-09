using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.EmailService.Models;

[Table("EmailThreads")]
public class EmailThread
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Subject { get; set; } = string.Empty;

    public string ParticipantEmails { get; set; } = string.Empty; // JSON array of email addresses

    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    public int EmailCount { get; set; } = 0;

    public bool IsArchived { get; set; } = false;

    public bool IsImportant { get; set; } = false;

    public bool HasUnread { get; set; } = false;

    [MaxLength(100)]
    public string? Category { get; set; } // RFQ, Quote, Order, Support, etc.

    public int? SupplierId { get; set; }

    public int? BuyerId { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }

    public string? Tags { get; set; } // JSON array of tags

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Email> Emails { get; set; } = new List<Email>();
}
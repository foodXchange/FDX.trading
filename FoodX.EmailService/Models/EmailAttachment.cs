using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.EmailService.Models;

[Table("EmailAttachments")]
public class EmailAttachment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int EmailId { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ContentType { get; set; }

    public long FileSize { get; set; }

    [MaxLength(500)]
    public string? BlobUrl { get; set; }

    [MaxLength(500)]
    public string? LocalPath { get; set; }

    public byte[]? Content { get; set; } // For small attachments, store in DB

    [MaxLength(255)]
    public string? ContentId { get; set; } // For inline attachments

    public bool IsInline { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("EmailId")]
    public virtual Email Email { get; set; } = null!;
}
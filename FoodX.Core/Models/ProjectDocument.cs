using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Core.Models
{
    [Table("ProjectDocuments")]
    public class ProjectDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        [MaxLength(100)]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string DocumentName { get; set; } = string.Empty;

        // File Information
        [Required]
        [MaxLength(500)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string FilePath { get; set; } = string.Empty;

        public long? FileSize { get; set; }

        [MaxLength(100)]
        public string? MimeType { get; set; }

        // Metadata
        [MaxLength(500)]
        public string? Description { get; set; }

        public int Version { get; set; } = 1;

        public bool IsLatestVersion { get; set; } = true;

        // Security
        public bool IsConfidential { get; set; } = false;

        [MaxLength(50)]
        public string AccessLevel { get; set; } = "Team"; // Public, Team, Restricted

        // Tracking
        [Required]
        public int UploadedBy { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } = null!;
    }
}
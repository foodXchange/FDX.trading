using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Core.Models
{
    [Table("ProjectActivities")]
    public class ProjectActivity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ActivityType { get; set; } = string.Empty;

        [Required]
        public string ActivityDescription { get; set; } = string.Empty;

        // Context
        [MaxLength(50)]
        public string? EntityType { get; set; } // 'RFQ', 'Quote', 'Document', etc.

        public int? EntityId { get; set; }

        // User Information
        [Required]
        public int UserId { get; set; }

        [MaxLength(100)]
        public string? UserRole { get; set; }

        // Metadata
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        // Timestamp
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } = null!;
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Core.Models
{
    [Table("ProjectNotifications")]
    public class ProjectNotification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        public int UserId { get; set; }

        // Notification Details
        [Required]
        [MaxLength(100)]
        public string NotificationType { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Message { get; set; }

        // Priority and Status
        [MaxLength(20)]
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent

        public bool IsRead { get; set; } = false;
        public bool IsEmailSent { get; set; } = false;

        // Action
        public bool ActionRequired { get; set; } = false;

        [MaxLength(500)]
        public string? ActionUrl { get; set; }

        public DateTime? ActionDeadline { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
        public DateTime? EmailSentAt { get; set; }

        // Navigation Properties
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } = null!;
    }
}
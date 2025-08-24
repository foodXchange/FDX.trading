using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Core.Models
{
    [Table("ProjectMessages")]
    public class ProjectMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        public int? ParentMessageId { get; set; }

        // Message Details
        [MaxLength(200)]
        public string? Subject { get; set; }

        [Required]
        public string MessageBody { get; set; } = string.Empty;

        [MaxLength(50)]
        public string MessageType { get; set; } = "General"; // General, Question, Clarification, Update, Alert

        // Sender
        [Required]
        public int SenderId { get; set; }

        [MaxLength(100)]
        public string? SenderRole { get; set; }

        // Recipients
        [MaxLength(50)]
        public string RecipientType { get; set; } = "Team"; // Team, Specific, Buyer, Supplier

        [MaxLength(500)]
        public string? RecipientIds { get; set; } // JSON array of user IDs

        // Status
        public bool IsRead { get; set; } = false;
        public bool IsImportant { get; set; } = false;

        // Timestamps
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        // Navigation Properties
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } = null!;

        [ForeignKey("ParentMessageId")]
        public virtual ProjectMessage? ParentMessage { get; set; }

        public virtual ICollection<ProjectMessage> Replies { get; set; } = new List<ProjectMessage>();
    }
}
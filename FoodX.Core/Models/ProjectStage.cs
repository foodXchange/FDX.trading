using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Core.Models
{
    [Table("ProjectStages")]
    public class ProjectStage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        [MaxLength(100)]
        public string StageName { get; set; } = string.Empty;

        [Required]
        public int StageOrder { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Skipped, Failed

        // Stage Details
        [MaxLength(500)]
        public string? Description { get; set; }

        public int RequiredApprovals { get; set; } = 0;
        public int CurrentApprovals { get; set; } = 0;

        // Dates
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? DueDate { get; set; }

        // Assignment
        public int? AssignedToUserId { get; set; }

        [MaxLength(100)]
        public string? AssignedToTeam { get; set; }

        // Navigation Properties
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } = null!;
    }
}
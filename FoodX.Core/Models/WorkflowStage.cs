using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Core.Models
{
    [Table("WorkflowStages")]
    public class WorkflowStage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TemplateId { get; set; }

        [Required]
        [MaxLength(100)]
        public string StageName { get; set; } = string.Empty;

        [Required]
        public int StageOrder { get; set; }

        // Stage Configuration
        [MaxLength(500)]
        public string? Description { get; set; }

        public int? DefaultDuration { get; set; } // days

        public bool IsMandatory { get; set; } = true;

        public bool RequiresApproval { get; set; } = false;

        public int ApprovalLevel { get; set; } = 1;

        // Auto-assignment
        [MaxLength(100)]
        public string? DefaultAssigneeRole { get; set; }

        // Navigation Properties
        [ForeignKey("TemplateId")]
        public virtual WorkflowTemplate Template { get; set; } = null!;
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Core.Models
{
    [Table("ProjectTeamMembers")]
    public class ProjectTeamMember
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Role { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Permissions { get; set; } = "View"; // View, Edit, Approve, Admin

        // Assignment Details
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public int AssignedBy { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation Properties
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } = null!;
    }
}
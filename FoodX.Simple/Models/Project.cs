using System.ComponentModel.DataAnnotations;

namespace FoodX.Simple.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string ProjectNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Status { get; set; } = "Planning";

        [MaxLength(50)]
        public string Priority { get; set; } = "Medium";

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime? ExpectedEndDate { get; set; }

        public DateTime? ActualEndDate { get; set; }

        [MaxLength(100)]
        public string AssignedTo { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Notes { get; set; }

        // Foreign key to RFQ
        public int RFQId { get; set; }
        public virtual RFQ RFQ { get; set; } = null!;

        // Tracking fields
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? ModifiedDate { get; set; }

        [MaxLength(100)]
        public string? ModifiedBy { get; set; }
    }
}
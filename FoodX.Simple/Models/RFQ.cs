using System.ComponentModel.DataAnnotations;

namespace FoodX.Simple.Models
{
    public class RFQ
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string RFQNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(100)]
        public string? PackageSize { get; set; }

        [MaxLength(100)]
        public string? CountryOfOrigin { get; set; }

        public bool IsKosherCertified { get; set; }

        [MaxLength(100)]
        public string? KosherOrganization { get; set; }

        [MaxLength(500)]
        public string? SpecialAttributes { get; set; }

        [MaxLength(2000)]
        public string? AdditionalNotes { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        public DateTime ResponseDeadline { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Active";

        public int ResponseCount { get; set; } = 0;

        // Foreign key to ProductBrief
        public int ProductBriefId { get; set; }
        public virtual ProductBrief ProductBrief { get; set; } = null!;

        // Tracking fields
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? ModifiedDate { get; set; }

        [MaxLength(100)]
        public string? ModifiedBy { get; set; }
    }
}
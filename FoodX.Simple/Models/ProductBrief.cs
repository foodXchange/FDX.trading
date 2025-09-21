using System.ComponentModel.DataAnnotations;

namespace FoodX.Simple.Models
{
    public class ProductBrief
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? BenchmarkBrandReference { get; set; }

        [MaxLength(500)]
        public string? BenchmarkWebsiteUrl { get; set; }

        [MaxLength(100)]
        public string? PackageSize { get; set; }

        [MaxLength(200)]
        public string? StorageRequirements { get; set; }

        [MaxLength(100)]
        public string? CountryOfOrigin { get; set; }

        // Kosher Certification
        public bool IsKosherCertified { get; set; }

        [MaxLength(100)]
        public string? KosherOrganization { get; set; }

        [MaxLength(100)]
        public string? KosherSymbol { get; set; }

        // Special Dietary Attributes (stored as comma-separated for simplicity)
        [MaxLength(500)]
        public string? SpecialAttributes { get; set; }

        // Image fields
        [MaxLength(500)]
        public string? ImagePath { get; set; }

        [MaxLength(1000)]
        public string? ImageUrl { get; set; }

        // Additional fields
        [MaxLength(2000)]
        public string? AdditionalNotes { get; set; }

        // Tracking fields
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? ModifiedDate { get; set; }

        [MaxLength(100)]
        public string? ModifiedBy { get; set; }

        // Status tracking
        [MaxLength(50)]
        public string Status { get; set; } = "Draft";

        // Workflow tracking
        public bool IsWorkflowCompleted { get; set; } = false;
        public DateTime? WorkflowCompletedDate { get; set; }

        // Navigation properties for workflow
        public virtual RFQ? GeneratedRFQ { get; set; }
    }
}
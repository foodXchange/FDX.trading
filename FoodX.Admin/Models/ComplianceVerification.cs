using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Admin.Models
{
    /// <summary>
    /// Tracks compliance and certification verification before purchase orders
    /// </summary>
    [Table("ComplianceVerifications")]
    public class ComplianceVerification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string VerificationNumber { get; set; } = string.Empty;

        // Context
        [Required]
        public int BuyerId { get; set; }

        [ForeignKey("BuyerId")]
        public virtual FoodXBuyer Buyer { get; set; }

        [Required]
        public int SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public virtual FoodXSupplier Supplier { get; set; }

        // Link to Quote/RFQ
        public int? SupplierQuoteId { get; set; }

        [ForeignKey("SupplierQuoteId")]
        public virtual SupplierQuote? SupplierQuote { get; set; }

        public int? RFQId { get; set; }

        [ForeignKey("RFQId")]
        public virtual RFQ? RFQ { get; set; }

        // Product Information
        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ProductCode { get; set; }

        [MaxLength(100)]
        public string? ProductCategory { get; set; }

        // Buyer Country Regulations
        [Required]
        [MaxLength(100)]
        public string BuyerCountry { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? RegulatoryAuthority { get; set; } // e.g., "FDA", "EFSA", "FSA"

        // Verification Status
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, InReview, Approved, Rejected, Expired, Conditional

        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }

        [MaxLength(100)]
        public string? ApprovedBy { get; set; }

        [MaxLength(1000)]
        public string? RejectionReason { get; set; }

        // Required Certifications
        public bool RequiresKosher { get; set; }
        public bool RequiresHalal { get; set; }
        public bool RequiresOrganic { get; set; }
        public bool RequiresGlutenFree { get; set; }
        public bool RequiresNonGMO { get; set; }
        public bool RequiresFairTrade { get; set; }
        public bool RequiresISO22000 { get; set; }
        public bool RequiresHACCP { get; set; }
        public bool RequiresBRC { get; set; }
        public bool RequiresFDA { get; set; }

        [MaxLength(1000)]
        public string? OtherRequiredCertifications { get; set; }

        // Certification Details
        public virtual ICollection<CertificationDocument> Certifications { get; set; } = new List<CertificationDocument>();

        // Compliance Checklist
        public virtual ICollection<ComplianceChecklistItem> ChecklistItems { get; set; } = new List<ComplianceChecklistItem>();

        // Laboratory Testing
        public bool RequiresLabTesting { get; set; }

        [MaxLength(500)]
        public string? RequiredTests { get; set; } // e.g., "Microbiological, Heavy Metals, Pesticide Residue"

        public virtual ICollection<LabTestResult> LabTests { get; set; } = new List<LabTestResult>();

        // Validity Period
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }

        // Conditional Approval
        public bool IsConditional { get; set; }

        [MaxLength(1000)]
        public string? Conditions { get; set; }

        public DateTime? ConditionDeadline { get; set; }

        // Notes
        [MaxLength(2000)]
        public string? InternalNotes { get; set; }

        [MaxLength(1000)]
        public string? SupplierNotes { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        // Methods
        public bool IsCurrentlyValid()
        {
            var now = DateTime.UtcNow;
            return Status == "Approved" &&
                   ValidFrom <= now &&
                   ValidUntil >= now;
        }

        public double GetComplianceScore()
        {
            if (ChecklistItems?.Any() != true)
                return 0;

            var completed = ChecklistItems.Count(c => c.IsCompleted);
            var total = ChecklistItems.Count;
            return (double)completed / total * 100;
        }
    }

    [Table("CertificationDocuments")]
    public class CertificationDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ComplianceVerificationId { get; set; }

        [ForeignKey("ComplianceVerificationId")]
        public virtual ComplianceVerification ComplianceVerification { get; set; }

        [Required]
        [MaxLength(100)]
        public string CertificationType { get; set; } = string.Empty; // Kosher, Halal, Organic, ISO22000, etc.

        [Required]
        [MaxLength(200)]
        public string CertificateName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? CertificateNumber { get; set; }

        [MaxLength(200)]
        public string? IssuingAuthority { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        [MaxLength(500)]
        public string DocumentUrl { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? DocumentHash { get; set; } // For integrity verification

        // Verification Status
        [Required]
        [MaxLength(50)]
        public string VerificationStatus { get; set; } = "Pending"; // Pending, Verified, Rejected, Expired

        public DateTime? VerifiedAt { get; set; }

        [MaxLength(100)]
        public string? VerifiedBy { get; set; }

        [MaxLength(500)]
        public string? VerificationNotes { get; set; }

        // Audit
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? UploadedBy { get; set; }

        public bool IsDeleted { get; set; } = false;
    }

    [Table("ComplianceChecklistItems")]
    public class ComplianceChecklistItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ComplianceVerificationId { get; set; }

        [ForeignKey("ComplianceVerificationId")]
        public virtual ComplianceVerification ComplianceVerification { get; set; }

        [Required]
        [MaxLength(200)]
        public string RequirementName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? RequirementDescription { get; set; }

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty; // Documentation, Quality, Safety, Legal, Environmental

        [Required]
        public bool IsMandatory { get; set; } = true;

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        [MaxLength(100)]
        public string? CompletedBy { get; set; }

        [MaxLength(500)]
        public string? Evidence { get; set; } // Link to document or reference

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("LabTestResults")]
    public class LabTestResult
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ComplianceVerificationId { get; set; }

        [ForeignKey("ComplianceVerificationId")]
        public virtual ComplianceVerification ComplianceVerification { get; set; }

        [Required]
        [MaxLength(100)]
        public string TestType { get; set; } = string.Empty; // Microbiological, Chemical, Physical

        [Required]
        [MaxLength(200)]
        public string TestName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? TestMethod { get; set; }

        [Required]
        [MaxLength(200)]
        public string Laboratory { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? LabCertification { get; set; } // e.g., "ISO 17025"

        [Required]
        public DateTime TestDate { get; set; }

        [MaxLength(100)]
        public string? SampleReference { get; set; }

        // Results
        [Required]
        [MaxLength(500)]
        public string Result { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Unit { get; set; }

        [MaxLength(100)]
        public string? AcceptableRange { get; set; }

        [Required]
        public bool PassedTest { get; set; }

        // Report
        [Required]
        [MaxLength(500)]
        public string ReportUrl { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ReportNumber { get; set; }

        // Audit
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? UploadedBy { get; set; }

        public bool IsDeleted { get; set; } = false;
    }

    public static class ComplianceStatus
    {
        public const string Pending = "Pending";
        public const string InReview = "InReview";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Expired = "Expired";
        public const string Conditional = "Conditional";
    }

    public static class ChecklistCategory
    {
        public const string Documentation = "Documentation";
        public const string Quality = "Quality";
        public const string Safety = "Safety";
        public const string Legal = "Legal";
        public const string Environmental = "Environmental";
        public const string Social = "Social";
    }
}
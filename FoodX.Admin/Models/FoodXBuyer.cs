using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Admin.Models
{
    [Table("FoodXBuyers")]
    public class FoodXBuyer
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(200)]
        public string? Company { get; set; }

        [MaxLength(100)]
        public string? Type { get; set; }

        [MaxLength(500)]
        public string? Website { get; set; }

        [MaxLength(200)]
        public string? Categories { get; set; }

        [MaxLength(50)]
        public string? Size { get; set; }

        [MaxLength(100)]
        public string? Stores { get; set; }

        [MaxLength(100)]
        public string? Region { get; set; }

        [MaxLength(200)]
        public string? Markets { get; set; }

        [MaxLength(200)]
        public string? Domain { get; set; }

        [MaxLength(200)]
        public string? KeyContact { get; set; }

        [MaxLength(100)]
        public string? Title { get; set; }

        [MaxLength(200)]
        public string? ContactEmail { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(50)]
        public string? Mobile { get; set; }

        [MaxLength(200)]
        public string? ProcurementContact { get; set; }

        [MaxLength(200)]
        public string? ProcurementEmail { get; set; }

        [MaxLength(200)]
        public string? DistributionCenter { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? MainCategory { get; set; }

        [MaxLength(500)]
        public string? SubCategories { get; set; }

        [MaxLength(100)]
        public string? TargetMarket { get; set; }

        [MaxLength(100)]
        public string? ImportCountries { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime? LastContact { get; set; }

        [MaxLength(100)]
        public string? Status { get; set; }

        [MaxLength(100)]
        public string? AccountManager { get; set; }

        public int? AnnualVolume { get; set; }

        [MaxLength(100)]
        public string? PreferredSuppliers { get; set; }

        // AI-Enhanced Fields
        public int? BusinessTypeScore { get; set; }
        
        public int? PurchasingPowerScore { get; set; }
        
        public int? PriceSensitivityIndex { get; set; }
        
        [MaxLength(100)]
        public string? MinimumOrderValue { get; set; }
        
        [MaxLength(200)]
        public string? PaymentTerms { get; set; }
        
        public float? DataCompletenessScore { get; set; }
        
        public int? EngagementReadinessScore { get; set; }
        
        public bool? AiReady { get; set; }
        
        [MaxLength(50)]
        public string? VerificationStatus { get; set; }
        
        public int? FoundedYear { get; set; }
        
        [MaxLength(100)]
        public string? AnnualRevenue { get; set; }
        
        public int? EmployeeCount { get; set; }
        
        public int? StoreCount { get; set; }
        
        [MaxLength(200)]
        public string? FoodFocus { get; set; }
        
        public string? AboutCompany { get; set; }
        
        [MaxLength(500)]
        public string? CertificationsRequired { get; set; }
        
        public string? ProductCategories { get; set; }
        
        [MaxLength(100)]
        public string? HqCity { get; set; }
        
        [MaxLength(200)]
        public string? PrimaryContactName { get; set; }
        
        [MaxLength(200)]
        public string? PrimaryContactTitle { get; set; }
        
        [MaxLength(200)]
        public string? PrimaryContactEmail { get; set; }
        
        [MaxLength(20)]
        public string? BuyerId { get; set; }

        // Computed properties for display
        [NotMapped]
        public string DisplayName => Company ?? "Unknown Company";

        [NotMapped]
        public string? Email => ContactEmail; // Alias for compatibility

        [NotMapped]
        public string ContactInfo => string.IsNullOrEmpty(ProcurementEmail) ? ContactEmail ?? "" : ProcurementEmail;

        [NotMapped]
        public bool HasContactInfo => !string.IsNullOrEmpty(ProcurementEmail) || !string.IsNullOrEmpty(ContactEmail);
    }
}
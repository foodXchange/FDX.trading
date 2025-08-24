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

        // Computed properties for display
        [NotMapped]
        public string DisplayName => Company ?? "Unknown Company";

        [NotMapped]
        public string ContactInfo => string.IsNullOrEmpty(ProcurementEmail) ? ContactEmail ?? "" : ProcurementEmail;

        [NotMapped]
        public bool HasContactInfo => !string.IsNullOrEmpty(ProcurementEmail) || !string.IsNullOrEmpty(ContactEmail);
    }
}
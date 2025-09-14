using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Admin.Models
{
    /// <summary>
    /// Stores confidential, buyer-specific pricing that is never visible to other buyers
    /// </summary>
    [Table("BuyerSpecificPricing")]
    public class BuyerSpecificPricing
    {
        [Key]
        public int Id { get; set; }

        // Buyer Information
        [Required]
        public int BuyerId { get; set; }

        [ForeignKey("BuyerId")]
        public virtual FoodXBuyer Buyer { get; set; }

        // Supplier Information
        [Required]
        public int SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public virtual FoodXSupplier Supplier { get; set; }

        // Product Information
        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ProductCode { get; set; }

        [MaxLength(100)]
        public string? SupplierProductCode { get; set; }

        // Link to catalog if exists
        public int? SupplierProductCatalogId { get; set; }

        [ForeignKey("SupplierProductCatalogId")]
        public virtual SupplierProductCatalog? CatalogProduct { get; set; }

        // Confidential Pricing Details
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal NegotiatedUnitPrice { get; set; }

        [Required]
        [MaxLength(50)]
        public string PriceUnit { get; set; } = "KG"; // KG, L, UNIT, CARTON, PALLET

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "EUR";

        // Volume-based Tier Pricing
        public virtual ICollection<BuyerPriceTier> PriceTiers { get; set; } = new List<BuyerPriceTier>();

        // Logistics Costs (Buyer-specific)
        [Column(TypeName = "decimal(18,2)")]
        public decimal? FreightCostPer20ft { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? FreightCostPer40ft { get; set; }

        [MaxLength(50)]
        public string? Incoterms { get; set; } // EXW, FOB, CIF, DDP

        [MaxLength(100)]
        public string? DeliveryPort { get; set; }

        // Payment Terms (Negotiated)
        [MaxLength(200)]
        public string? PaymentTerms { get; set; } // "NET30", "2/10 NET30", "LC at sight"

        [Column(TypeName = "decimal(5,2)")]
        public decimal? EarlyPaymentDiscountPercent { get; set; }

        // Validity Period
        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidUntil { get; set; }

        // Minimum Order Requirements
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinimumOrderQuantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinimumOrderValue { get; set; }

        // Status and Approval
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, UnderNegotiation, Approved, Active, Expired, Suspended

        public DateTime? ApprovedAt { get; set; }

        [MaxLength(100)]
        public string? ApprovedBy { get; set; }

        // Negotiation Reference
        public int? NegotiationHistoryId { get; set; }

        // Special Terms
        [MaxLength(1000)]
        public string? SpecialTerms { get; set; }

        [MaxLength(500)]
        public string? ExclusivityTerms { get; set; } // e.g., "Exclusive price for buyer in Region X"

        // Security & Confidentiality
        [Required]
        public bool IsConfidential { get; set; } = true;

        [Required]
        [MaxLength(50)]
        public string ConfidentialityLevel { get; set; } = "Strict"; // Strict, Standard, Public

        // Audit Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Methods
        public bool IsCurrentlyValid()
        {
            var now = DateTime.UtcNow;
            return Status == "Active" &&
                   ValidFrom <= now &&
                   ValidUntil >= now &&
                   !IsDeleted;
        }

        public decimal GetPriceForQuantity(decimal quantity)
        {
            // Check tier pricing first
            var applicableTier = PriceTiers?
                .Where(t => t.MinQuantity <= quantity &&
                           (t.MaxQuantity == null || t.MaxQuantity >= quantity))
                .OrderByDescending(t => t.MinQuantity)
                .FirstOrDefault();

            if (applicableTier != null)
            {
                return applicableTier.UnitPrice;
            }

            return NegotiatedUnitPrice;
        }
    }

    [Table("BuyerPriceTiers")]
    public class BuyerPriceTier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BuyerSpecificPricingId { get; set; }

        [ForeignKey("BuyerSpecificPricingId")]
        public virtual BuyerSpecificPricing BuyerPricing { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MinQuantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxQuantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? DiscountPercent { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public static class PricingStatus
    {
        public const string Draft = "Draft";
        public const string UnderNegotiation = "UnderNegotiation";
        public const string Approved = "Approved";
        public const string Active = "Active";
        public const string Expired = "Expired";
        public const string Suspended = "Suspended";
    }

    public static class ConfidentialityLevel
    {
        public const string Strict = "Strict";     // Never share, buyer-eyes only
        public const string Standard = "Standard"; // Normal confidentiality
        public const string Public = "Public";     // Can be shown in catalogs
    }
}
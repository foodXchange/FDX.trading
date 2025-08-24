using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Core.Models
{
    [Table("SupplierOffers")]
    public class SupplierOffer
    {
        [Key]
        public int Id { get; set; }

        public int? ProjectId { get; set; }

        [Required]
        [MaxLength(50)]
        public string OfferNumber { get; set; } = string.Empty;

        // Supplier Information
        [Required]
        public int SupplierId { get; set; }

        public int? SupplierCompanyId { get; set; }

        // Offer Details
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        // Validity
        public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ValidUntil { get; set; }

        // Terms
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinimumOrderValue { get; set; }

        [MaxLength(500)]
        public string? DeliveryTerms { get; set; }

        [MaxLength(500)]
        public string? PaymentTerms { get; set; }

        // Status
        [MaxLength(50)]
        public string Status { get; set; } = "Active"; // Draft, Active, Expired, Withdrawn

        public bool IsPublic { get; set; } = true;

        // Targeting
        [MaxLength(500)]
        public string? TargetBuyerTypes { get; set; } // JSON array of buyer types

        [MaxLength(500)]
        public string? TargetRegions { get; set; } // JSON array of regions

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("ProjectId")]
        public virtual Project? Project { get; set; }
    }
}
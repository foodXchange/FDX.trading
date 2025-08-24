using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Core.Models
{
    [Table("RFQItems")]
    public class RFQItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RFQId { get; set; }

        [Required]
        public int ItemNumber { get; set; }

        // Product Information
        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ProductCategory { get; set; }

        public string? Description { get; set; }

        public string? Specifications { get; set; }

        // Quantity and Unit
        [Required]
        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [Required]
        [MaxLength(50)]
        public string Unit { get; set; } = string.Empty;

        // Quality Requirements
        [MaxLength(500)]
        public string? QualityStandards { get; set; }

        [MaxLength(500)]
        public string? CertificationsRequired { get; set; }

        // Delivery
        public DateTime? RequiredDeliveryDate { get; set; }

        [MaxLength(100)]
        public string? DeliveryFrequency { get; set; } // For recurring orders

        // Navigation Properties
        [ForeignKey("RFQId")]
        public virtual RFQ RFQ { get; set; } = null!;

        public virtual ICollection<QuoteItem> QuoteItems { get; set; } = new List<QuoteItem>();
    }
}
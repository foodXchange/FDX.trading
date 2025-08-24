using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Core.Models
{
    [Table("QuoteItems")]
    public class QuoteItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuoteId { get; set; }

        [Required]
        public int RFQItemId { get; set; }

        // Pricing
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        // Product Details
        [MaxLength(200)]
        public string? OfferedProductName { get; set; }

        public string? OfferedProductDescription { get; set; }

        // Delivery
        public int? DeliveryLeadTime { get; set; } // days

        [Column(TypeName = "decimal(18,3)")]
        public decimal? AvailableQuantity { get; set; }

        // Notes
        public string? Notes { get; set; }

        // Navigation Properties
        [ForeignKey("QuoteId")]
        public virtual Quote Quote { get; set; } = null!;

        [ForeignKey("RFQItemId")]
        public virtual RFQItem RFQItem { get; set; } = null!;
    }
}
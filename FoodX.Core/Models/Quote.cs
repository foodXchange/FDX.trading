using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Core.Models
{
    [Table("Quotes")]
    public class Quote
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RFQId { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        [MaxLength(50)]
        public string QuoteNumber { get; set; } = string.Empty;

        // Supplier Information
        [Required]
        public int SupplierId { get; set; }

        public int? SupplierCompanyId { get; set; }

        // Quote Details
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; } = "USD";

        [Required]
        public DateTime ValidUntil { get; set; }

        // Terms
        [MaxLength(500)]
        public string? DeliveryTerms { get; set; }

        [MaxLength(500)]
        public string? PaymentTerms { get; set; }

        [MaxLength(500)]
        public string? WarrantyTerms { get; set; }

        // Status
        [MaxLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Submitted, UnderReview, Shortlisted, Accepted, Rejected, Withdrawn

        // Evaluation
        [Column(TypeName = "decimal(5,2)")]
        public decimal? TechnicalScore { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? CommercialScore { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? OverallScore { get; set; }

        public string? EvaluationNotes { get; set; }

        // Timestamps
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime? DecisionAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("RFQId")]
        public virtual RFQ RFQ { get; set; } = null!;

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } = null!;

        public virtual ICollection<QuoteItem> Items { get; set; } = new List<QuoteItem>();
    }
}
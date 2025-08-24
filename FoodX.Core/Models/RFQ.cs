using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Core.Models
{
    [Table("RFQs")]
    public class RFQ
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RFQNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Buyer Information
        [Required]
        public int BuyerId { get; set; }

        public int? BuyerCompanyId { get; set; }

        // RFQ Details
        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(500)]
        public string? DeliveryLocation { get; set; }

        [MaxLength(200)]
        public string? DeliveryTerms { get; set; }

        [MaxLength(200)]
        public string? PaymentTerms { get; set; }

        // Dates
        public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime SubmissionDeadline { get; set; }

        public int QuotationValidityPeriod { get; set; } = 30; // days

        // Status
        [MaxLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Published, Closed, Awarded, Cancelled

        public bool IsPublic { get; set; } = false;

        // Budget
        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedBudget { get; set; }

        public bool ShowBudgetToSuppliers { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } = null!;

        public virtual ICollection<RFQItem> Items { get; set; } = new List<RFQItem>();
        public virtual ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    }
}
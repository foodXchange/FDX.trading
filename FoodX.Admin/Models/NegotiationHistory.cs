using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Admin.Models
{
    /// <summary>
    /// Tracks the complete history of price negotiations between buyers and suppliers
    /// </summary>
    [Table("NegotiationHistory")]
    public class NegotiationHistory
    {
        [Key]
        public int Id { get; set; }

        // Negotiation Context
        [Required]
        [MaxLength(50)]
        public string NegotiationNumber { get; set; } = string.Empty;

        [Required]
        public int BuyerId { get; set; }

        [ForeignKey("BuyerId")]
        public virtual FoodXBuyer Buyer { get; set; }

        [Required]
        public int SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public virtual FoodXSupplier Supplier { get; set; }

        // Link to RFQ/Quote if applicable
        public int? RFQId { get; set; }

        [ForeignKey("RFQId")]
        public virtual RFQ? RFQ { get; set; }

        public int? SupplierQuoteId { get; set; }

        [ForeignKey("SupplierQuoteId")]
        public virtual SupplierQuote? SupplierQuote { get; set; }

        // Product Being Negotiated
        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ProductCode { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }

        [Required]
        [MaxLength(50)]
        public string Unit { get; set; } = "KG";

        // Negotiation Timeline
        [Required]
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "InProgress"; // InProgress, Agreed, Rejected, Expired, OnHold

        // Initial and Final Prices
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal InitialPrice { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? FinalAgreedPrice { get; set; }

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "EUR";

        // Negotiation Rounds
        public virtual ICollection<NegotiationRound> Rounds { get; set; } = new List<NegotiationRound>();

        // Communication Summary
        public int TotalRounds { get; set; } = 0;
        public int EmailCount { get; set; } = 0;
        public int PhoneCallCount { get; set; } = 0;
        public int MeetingCount { get; set; } = 0;

        // Final Terms
        [MaxLength(1000)]
        public string? FinalTerms { get; set; }

        [MaxLength(100)]
        public string? FinalIncoterms { get; set; }

        [MaxLength(200)]
        public string? FinalPaymentTerms { get; set; }

        public DateTime? FinalPriceValidUntil { get; set; }

        // Participants
        [MaxLength(100)]
        public string? BuyerNegotiator { get; set; }

        [MaxLength(100)]
        public string? SupplierNegotiator { get; set; }

        // Notes
        [MaxLength(2000)]
        public string? InternalNotes { get; set; } // Visible only to buyer

        [MaxLength(2000)]
        public string? SharedNotes { get; set; } // Visible to both parties

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        // Methods
        public decimal CalculatePriceReduction()
        {
            if (FinalAgreedPrice.HasValue && InitialPrice > 0)
            {
                return ((InitialPrice - FinalAgreedPrice.Value) / InitialPrice) * 100;
            }
            return 0;
        }

        public TimeSpan GetNegotiationDuration()
        {
            return (CompletedAt ?? DateTime.UtcNow) - StartedAt;
        }
    }

    /// <summary>
    /// Individual negotiation round/interaction
    /// </summary>
    [Table("NegotiationRounds")]
    public class NegotiationRound
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NegotiationHistoryId { get; set; }

        [ForeignKey("NegotiationHistoryId")]
        public virtual NegotiationHistory NegotiationHistory { get; set; }

        [Required]
        public int RoundNumber { get; set; }

        [Required]
        public DateTime OccurredAt { get; set; }

        // Communication Details
        [Required]
        [MaxLength(50)]
        public string CommunicationMethod { get; set; } // Email, Phone, Meeting, Portal, WhatsApp

        [MaxLength(100)]
        public string? CommunicationReference { get; set; } // Email ID, Call ID, Meeting ID

        // Price Movement
        [Column(TypeName = "decimal(18,4)")]
        public decimal? ProposedPriceBuyer { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? ProposedPriceSupplier { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? AgreedPrice { get; set; } // If agreed in this round

        // Terms Discussed
        [MaxLength(500)]
        public string? QuantityDiscussed { get; set; }

        [MaxLength(500)]
        public string? DeliveryTermsDiscussed { get; set; }

        [MaxLength(500)]
        public string? PaymentTermsDiscussed { get; set; }

        [MaxLength(500)]
        public string? QualityRequirementsDiscussed { get; set; }

        // Discussion Summary
        [MaxLength(2000)]
        public string? DiscussionSummary { get; set; }

        [MaxLength(1000)]
        public string? BuyerPosition { get; set; }

        [MaxLength(1000)]
        public string? SupplierPosition { get; set; }

        // Outcome
        [Required]
        [MaxLength(50)]
        public string Outcome { get; set; } // Continue, PriceAgreed, TermsAgreed, Deadlock, OnHold

        [MaxLength(500)]
        public string? NextSteps { get; set; }

        // Documents
        [MaxLength(1000)]
        public string? AttachedDocuments { get; set; } // JSON array of document URLs

        // Participants
        [MaxLength(200)]
        public string? BuyerParticipants { get; set; }

        [MaxLength(200)]
        public string? SupplierParticipants { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public bool IsDeleted { get; set; } = false;
    }

    /// <summary>
    /// Template for negotiation emails
    /// </summary>
    [Table("NegotiationEmailTemplates")]
    public class NegotiationEmailTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string TemplateName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TemplateType { get; set; } // InitialOffer, CounterOffer, Acceptance, Rejection

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Variables { get; set; } // JSON array of variable names

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public static class NegotiationStatus
    {
        public const string InProgress = "InProgress";
        public const string Agreed = "Agreed";
        public const string Rejected = "Rejected";
        public const string Expired = "Expired";
        public const string OnHold = "OnHold";
    }

    public static class CommunicationMethod
    {
        public const string Email = "Email";
        public const string Phone = "Phone";
        public const string Meeting = "Meeting";
        public const string Portal = "Portal";
        public const string WhatsApp = "WhatsApp";
        public const string VideoCall = "VideoCall";
    }

    public static class RoundOutcome
    {
        public const string Continue = "Continue";
        public const string PriceAgreed = "PriceAgreed";
        public const string TermsAgreed = "TermsAgreed";
        public const string Deadlock = "Deadlock";
        public const string OnHold = "OnHold";
    }
}
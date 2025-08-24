using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Core.Models
{
    [Table("Projects")]
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProjectNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ProjectName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ProjectType { get; set; } = "BuyerRFQ"; // BuyerRFQ, SupplierOffer, DirectNegotiation

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Draft";

        [MaxLength(20)]
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent

        // Initiator Information
        [Required]
        [MaxLength(20)]
        public string InitiatorType { get; set; } = string.Empty; // Buyer, Supplier

        [Required]
        public int InitiatorUserId { get; set; }

        public int? InitiatorCompanyId { get; set; }

        // Project Details
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalValue { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; } = "USD";

        // Dates
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        // Tracking
        [Required]
        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<ProjectStage> Stages { get; set; } = new List<ProjectStage>();
        public virtual ICollection<ProjectTeamMember> TeamMembers { get; set; } = new List<ProjectTeamMember>();
        public virtual ICollection<ProjectDocument> Documents { get; set; } = new List<ProjectDocument>();
        public virtual ICollection<ProjectActivity> Activities { get; set; } = new List<ProjectActivity>();
        public virtual ICollection<ProjectMessage> Messages { get; set; } = new List<ProjectMessage>();
        public virtual ICollection<RFQ> RFQs { get; set; } = new List<RFQ>();
        public virtual ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    }
}
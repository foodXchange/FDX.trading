using System.ComponentModel.DataAnnotations;

namespace FoodX.Core.Models.Entities;

public class Buyer
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Company { get; set; } = string.Empty;

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

    [EmailAddress]
    [MaxLength(200)]
    public string? ProcurementEmail { get; set; }

    [Phone]
    [MaxLength(50)]
    public string? ProcurementPhone { get; set; }

    [MaxLength(200)]
    public string? ProcurementManager { get; set; }

    [MaxLength(200)]
    public string? CommercialManager { get; set; }

    [MaxLength(200)]
    public string? ImportManager { get; set; }

    [MaxLength(200)]
    public string? SourcingManager { get; set; }

    [MaxLength(200)]
    public string? PurchasingManager { get; set; }

    [MaxLength(500)]
    public string? LinkedinCompany { get; set; }

    [MaxLength(500)]
    public string? VendorPortal { get; set; }

    public string? SupplierRequirements { get; set; }

    [MaxLength(500)]
    public string? CertificationsRequired { get; set; }

    [MaxLength(100)]
    public string? MinimumOrder { get; set; }

    [MaxLength(200)]
    public string? PaymentTerms { get; set; }

    public string? DeliveryRequirements { get; set; }

    [MaxLength(200)]
    public string? DietaryFocus { get; set; }

    [MaxLength(100)]
    public string? Segment { get; set; }

    [EmailAddress]
    [MaxLength(200)]
    public string? GeneralEmail { get; set; }

    [Phone]
    [MaxLength(50)]
    public string? GeneralPhone { get; set; }

    [MaxLength(50)]
    public string? ScrapeStatus { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public virtual ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    public virtual ICollection<RFQ> RFQs { get; set; } = new List<RFQ>();
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
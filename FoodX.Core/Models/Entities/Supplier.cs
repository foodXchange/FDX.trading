using System.ComponentModel.DataAnnotations;

namespace FoodX.Core.Models.Entities;

public class Supplier
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string SupplierName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? CompanyWebsite { get; set; }

    public string? Description { get; set; }

    public string? ProductCategory { get; set; }

    public string? Address { get; set; }

    [EmailAddress]
    [MaxLength(200)]
    public string? CompanyEmail { get; set; }

    [Phone]
    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? SourcingStages { get; set; }

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    public string? ProfileImages { get; set; }

    [MaxLength(200)]
    public string? PreferredSeaPort { get; set; }

    public bool? KosherCertification { get; set; }

    [MaxLength(100)]
    public string? VATNumber { get; set; }

    [MaxLength(100)]
    public string? SupplierNumber { get; set; }

    [MaxLength(200)]
    public string? PaymentTerms { get; set; }

    [MaxLength(100)]
    public string? Incoterms { get; set; }

    [MaxLength(100)]
    public string? IncotermsPriceBase { get; set; }

    [MaxLength(50)]
    public string? SupplierCode { get; set; }

    public decimal? DistanceToSeaport { get; set; }

    public string? PickingAddress { get; set; }

    public string? Categories { get; set; }

    public string? ProductDetails { get; set; } // Renamed from Products to avoid conflict

    [MaxLength(200)]
    public string? SourceOfData { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsVerified { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<SupplierOffer> SupplierOffers { get; set; } = new List<SupplierOffer>();
    public virtual ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
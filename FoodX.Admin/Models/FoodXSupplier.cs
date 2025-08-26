using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Admin.Models;

[Table("FoodXSuppliers")]
public class FoodXSupplier
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string SupplierName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? CompanyWebsite { get; set; }

    public string? Description { get; set; }

    [MaxLength(200)]
    public string? ProductCategory { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? CompanyEmail { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    // Products is stored as text in the database
    public string? Products { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    public string? PaymentTerms { get; set; }

    public string? Incoterms { get; set; }

    public string? MinimumOrderQuantity { get; set; }

    public string? LeadTime { get; set; }

    public string? Certifications { get; set; }

    public string? QualityControl { get; set; }

    public string? PackagingOptions { get; set; }

    public string? ShippingMethods { get; set; }

    public string? ProductionCapacity { get; set; }

    public int? EstablishedYear { get; set; }

    public int? NumberOfEmployees { get; set; }

    public string? AnnualRevenue { get; set; }

    public decimal? ExportPercentage { get; set; }

    public string? MainMarkets { get; set; }

    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    public string? ContactPosition { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public string? Notes { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsVerified { get; set; }

    public DateTime? VerifiedDate { get; set; }

    public decimal? Rating { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    // Additional columns from the extended schema
    public string? ProductsList { get; set; }

    public string? BrandsList { get; set; }

    public string? Categories { get; set; }

    public string? KosherCertification { get; set; }

    public string? OtherCertifications { get; set; }

    public string? PrimaryEmail { get; set; }

    public string? AllEmails { get; set; }

    public string? PrimaryPhone { get; set; }

    public string? ClosestSeaPort { get; set; }

    public int? DistanceToSeaPort { get; set; }

    public string? SupplierCode { get; set; }

    public string? SupplierVATNumber { get; set; }

    public int? YearFounded { get; set; }

    public string? SupplierType { get; set; }

    public string? PickingAddress { get; set; }

    public string? IncotermsPrice { get; set; }

    public string? SourcingStages { get; set; }

    public string? ExtractionStatus { get; set; }

    public int? ProductsFound { get; set; }

    public int? BrandsFound { get; set; }

    public string? DataSource { get; set; }

    public DateTime? FirstCreated { get; set; }

    public DateTime? LastUpdated { get; set; }

    public string? CompanyLogoURL { get; set; }

    public string? ProfileImages { get; set; }

    // Certification boolean fields for optimized filtering
    public bool? IsKosherCertified { get; set; }

    public bool? IsHalalCertified { get; set; }

    public bool? IsOrganicCertified { get; set; }

    public bool? IsGlutenFreeCertified { get; set; }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FoodX.Core.Models.Base;

namespace FoodX.Core.Models.Entities;

public class Product : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? SKU { get; set; }
    
    public string? Description { get; set; }
    
    [MaxLength(100)]
    public string? Category { get; set; }
    
    [MaxLength(100)]
    public string? SubCategory { get; set; }
    
    [MaxLength(50)]
    public string? Unit { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; } // Changed from UnitPrice and made non-nullable
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? UnitPrice { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinOrderQuantity { get; set; }
    
    [MaxLength(100)]
    public string? PackagingType { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? PackageSize { get; set; }
    
    [MaxLength(50)]
    public string? PackageUnit { get; set; }
    
    public bool? IsKosher { get; set; }
    
    public bool? IsOrganic { get; set; }
    
    public bool? IsGlutenFree { get; set; }
    
    public bool? IsVegan { get; set; }
    
    [MaxLength(500)]
    public string? Certifications { get; set; }
    
    [MaxLength(100)]
    public string? CountryOfOrigin { get; set; }
    
    [MaxLength(100)]
    public string? Origin { get; set; } // Alias for CountryOfOrigin
    
    [MaxLength(100)]
    public string? Brand { get; set; }
    
    [MaxLength(100)]
    public string? ShelfLife { get; set; }
    
    public string? StorageRequirements { get; set; }
    
    [MaxLength(500)]
    public string? ImageUrl { get; set; }
    
    // Foreign key
    public int? SupplierId { get; set; }
    
    // Navigation properties
    [ForeignKey("SupplierId")]
    public virtual Supplier? Supplier { get; set; }
    
    public virtual ICollection<QuoteItem> QuoteItems { get; set; } = new List<QuoteItem>();
    public virtual ICollection<RFQItem> RFQItems { get; set; } = new List<RFQItem>();
}
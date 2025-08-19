using System.ComponentModel.DataAnnotations;

namespace FoodX.Admin.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = "";
        
        [MaxLength(50)]
        public string Category { get; set; } = "";
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        public decimal Price { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string Unit { get; set; } = "";
        
        public int MinOrderQuantity { get; set; } = 1;
        
        public int StockQuantity { get; set; }
        
        [MaxLength(50)]
        public string? SKU { get; set; }
        
        [MaxLength(100)]
        public string? Origin { get; set; }
        
        public bool IsOrganic { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        public bool IsActive { get; set; } = true;
        
        [MaxLength(500)]
        public string? ImageUrl { get; set; }
        
        // Foreign key to Supplier
        public int? SupplierId { get; set; }
        public virtual Supplier? Supplier { get; set; }
        
        // Foreign key to Company
        public int? CompanyId { get; set; }
        public virtual Company? Company { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
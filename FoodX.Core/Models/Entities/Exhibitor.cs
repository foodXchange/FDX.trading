using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Core.Models.Entities;

public class Exhibitor
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? ProfileUrl { get; set; }
    
    [MaxLength(100)]
    public string? Country { get; set; }
    
    public string? Products { get; set; }
    
    [MaxLength(500)]
    public string? Contact { get; set; }
    
    [EmailAddress]
    [MaxLength(200)]
    public string? Email { get; set; }
    
    [Phone]
    [MaxLength(50)]
    public string? Phone { get; set; }
    
    [MaxLength(500)]
    public string? Website { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Foreign key
    public int ExhibitionId { get; set; }
    
    // Navigation properties
    [ForeignKey("ExhibitionId")]
    public virtual Exhibition Exhibition { get; set; } = null!;
    
    // Link to supplier if matched
    public int? SupplierId { get; set; }
    
    [ForeignKey("SupplierId")]
    public virtual Supplier? Supplier { get; set; }
}
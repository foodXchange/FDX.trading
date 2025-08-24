using System.ComponentModel.DataAnnotations;

namespace FoodX.Core.Models.Base;

public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    [MaxLength(100)]
    public string? CreatedBy { get; set; }
    
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }
}
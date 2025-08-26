using System.ComponentModel.DataAnnotations;

namespace FoodX.Core.Models.Entities;

public class Exhibition
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(200)]
    public string? City { get; set; }

    [MaxLength(500)]
    public string? Venue { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [MaxLength(500)]
    public string? Website { get; set; }

    [MaxLength(500)]
    public string? SourceUrl { get; set; }

    [MaxLength(100)]
    public string? Status { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Exhibitor> Exhibitors { get; set; } = new List<Exhibitor>();
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Admin.Models
{
    [Table("Agents")]
    public class Agent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [MaxLength(200)]
        public string? AgencyName { get; set; }

        [MaxLength(100)]
        public string? TerritoryRegion { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public FoodXUser User { get; set; } = null!;
    }
}
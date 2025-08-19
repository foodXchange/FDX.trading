using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Admin.Models
{
    [Table("Buyers")]
    public class Buyer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? CompanyId { get; set; }

        [MaxLength(100)]
        public string? Department { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public FoodXUser User { get; set; } = null!;

        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }
    }
}
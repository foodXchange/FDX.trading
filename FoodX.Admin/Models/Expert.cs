using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Admin.Models
{
    [Table("Experts")]
    public class Expert
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [MaxLength(200)]
        public string? Specialization { get; set; }

        [MaxLength(50)]
        public string? CertificationNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public FoodXUser User { get; set; } = null!;
    }
}
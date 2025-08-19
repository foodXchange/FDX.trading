using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Admin.Models
{
    [Table("UserPhones")]
    public class UserPhone
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [MaxLength(10)]
        public string? CountryCode { get; set; } = "+1";

        [Required]
        [MaxLength(50)]
        public string PhoneType { get; set; } = "Mobile"; // Mobile, Work, Home, Fax, Other

        [Required]
        [MaxLength(50)]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        // Navigation property
        [ForeignKey("UserId")]
        public FoodXUser User { get; set; } = null!;
    }
}
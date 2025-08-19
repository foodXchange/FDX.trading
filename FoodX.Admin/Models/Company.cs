using System.ComponentModel.DataAnnotations;

namespace FoodX.Admin.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string CompanyType { get; set; } = "Buyer"; // Buyer or Supplier

        [MaxLength(50)]
        public string? BuyerCategory { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        [MaxLength(200)]
        [Url]
        public string? Website { get; set; }

        [MaxLength(256)]
        [EmailAddress]
        public string? MainEmail { get; set; }

        [MaxLength(50)]
        [Phone]
        public string? MainPhone { get; set; }

        [MaxLength(50)]
        public string? VatNumber { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(300)]
        public string? WarehouseAddress { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<UserEmployment> UserEmployments { get; } = [];
    }
}
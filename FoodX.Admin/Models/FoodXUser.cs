using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Admin.Models
{
    [Table("Users")]
    public class FoodXUser
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(10)]
        public string? Title { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? PasswordHash { get; set; }

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "Buyer"; // Buyer, Seller, Agent, Admin

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public string FullName => $"{Title} {FirstName} {LastName}".Trim();

        // Navigation properties
        public ICollection<UserEmployment> UserEmployments { get; } = [];
        public ICollection<UserPhone> UserPhones { get; } = [];
    }
}
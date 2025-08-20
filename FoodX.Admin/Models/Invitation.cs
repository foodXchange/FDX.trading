using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Admin.Models
{
    [Table("Invitations")]
    public class Invitation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string InvitationCode { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(256)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? InvitedByEmail { get; set; }

        [MaxLength(50)]
        public string? InvitedByName { get; set; }

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "Buyer"; // Buyer, Supplier, Expert, Agent, Admin

        public int? CompanyId { get; set; }

        [MaxLength(200)]
        public string? CompanyName { get; set; }

        [MaxLength(500)]
        public string? Message { get; set; } // Optional welcome message

        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

        public DateTime? UsedAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public bool IsRevoked { get; set; } = false;

        [MaxLength(256)]
        public string? UsedByUserId { get; set; } // AspNetUsers Id

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        // Computed properties
        [NotMapped]
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;

        [NotMapped]
        public bool IsValid => !IsUsed && !IsRevoked && !IsExpired;

        [NotMapped]
        public string Status
        {
            get
            {
                if (IsUsed) return "Used";
                if (IsRevoked) return "Revoked";
                if (IsExpired) return "Expired";
                return "Active";
            }
        }
    }
}
using Microsoft.AspNetCore.Identity;

namespace FoodX.Admin.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CompanyName { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? Country { get; set; } // Country code (e.g., "US", "IL", "GB")
    public bool IsSuperAdmin { get; set; } = false;
    public string? ImpersonatedBy { get; set; } // UserId of admin impersonating this user
    public DateTime? LastLoginAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // For invitation tracking
    public string? InvitationCode { get; set; }
    public int? InvitationId { get; set; }
}


using Microsoft.AspNetCore.Identity;

namespace FoodX.Supplier.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyRole { get; set; }
    public string? Country { get; set; }
    public new string? PhoneNumber { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Link to FoodXSuppliers table
    public int? FoodXSupplierId { get; set; }
    
    // Profile completion tracking
    public bool ProfileCompleted { get; set; } = false;
    public DateTime? ProfileCompletedAt { get; set; }
}
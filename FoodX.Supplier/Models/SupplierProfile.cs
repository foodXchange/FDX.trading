using FoodX.Admin.Models;

namespace FoodX.Supplier.Models;

public class SupplierProfile : FoodXSupplier
{
    // Extended properties for supplier portal
    public string? City { get; set; }
    public string? Whatsapp { get; set; }
    public string? ProductTypes { get; set; }
    public string? ProductDetails { get; set; }
    public string? Markets { get; set; }
    
    // Social Media Links
    public string? LinkedInUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? YouTubeUrl { get; set; }
}
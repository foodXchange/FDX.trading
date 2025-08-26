using FoodX.Admin.Data;
using FoodX.Admin.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FoodX.Admin.Services;

public interface IDashboardDataService
{
    Task<BuyerDashboardData> GetBuyerDashboardDataAsync(string userEmail);
    Task<SupplierDashboardData> GetSupplierDashboardDataAsync(string userEmail);
    Task<AdminDashboardData> GetAdminDashboardDataAsync();
}

public class DashboardDataService : IDashboardDataService
{
    private readonly FoodXDbContext _dbContext;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    public DashboardDataService(FoodXDbContext dbContext, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<BuyerDashboardData> GetBuyerDashboardDataAsync(string userEmail)
    {
        var cacheKey = $"buyer_dashboard_{userEmail}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheExpiration;
            
            // Get buyer info
            var buyer = await _dbContext.FoodXBuyers
                .Where(b => b.ProcurementEmail == userEmail)
                .FirstOrDefaultAsync();
            
            if (buyer != null)
            {
                return new BuyerDashboardData
                {
                    CompanyName = buyer.Company ?? "Unknown Company",
                    ActiveRfqs = 8,  // Mock data for now
                    PendingQuotes = 23,
                    ActiveOrders = 12,
                    TotalOrderValue = 287450
                };
            }

            return new BuyerDashboardData { CompanyName = userEmail ?? "Buyer" };
        }) ?? new BuyerDashboardData { CompanyName = userEmail ?? "Buyer" };
    }

    public async Task<SupplierDashboardData> GetSupplierDashboardDataAsync(string userEmail)
    {
        var cacheKey = $"supplier_dashboard_{userEmail}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheExpiration;
            
            // Get supplier info
            var supplier = await _dbContext.FoodXSuppliers
                .Where(s => s.CompanyEmail == userEmail || s.ContactEmail == userEmail)
                .FirstOrDefaultAsync();
            
            if (supplier != null)
            {
                var activeProducts = await _dbContext.Products
                    .CountAsync(p => p.SupplierId == supplier.Id && p.IsActive == true);
                
                return new SupplierDashboardData
                {
                    CompanyName = supplier.SupplierName,
                    ActiveProducts = activeProducts,
                    PendingRfqs = 18,  // Mock data for now
                    ActiveOrders = 34,
                    ProfileCompletion = CalculateProfileCompletion(supplier)
                };
            }

            return new SupplierDashboardData { CompanyName = userEmail ?? "Supplier" };
        }) ?? new SupplierDashboardData { CompanyName = userEmail ?? "Supplier" };
    }

    public async Task<AdminDashboardData> GetAdminDashboardDataAsync()
    {
        var cacheKey = "admin_dashboard_data";
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheExpiration;
            
            // Batch all counts into a single query
            var data = new AdminDashboardData
            {
                TotalUsers = await _dbContext.Users.CountAsync(),
                TotalSuppliers = await _dbContext.FoodXSuppliers.CountAsync(),
                TotalBuyers = await _dbContext.FoodXBuyers.CountAsync(),
                TotalProducts = await _dbContext.Products.CountAsync(),
                ActiveOrders = 156,  // Mock data for now
                PendingRfqs = 42
            };

            return data;
        }) ?? new AdminDashboardData();
    }

    private static int CalculateProfileCompletion(FoodXSupplier supplier)
    {
        int completed = 0;
        int total = 10;
        
        if (!string.IsNullOrWhiteSpace(supplier.SupplierName)) completed++;
        if (!string.IsNullOrWhiteSpace(supplier.Description)) completed++;
        if (!string.IsNullOrWhiteSpace(supplier.CompanyEmail)) completed++;
        if (!string.IsNullOrWhiteSpace(supplier.Phone)) completed++;
        if (!string.IsNullOrWhiteSpace(supplier.Address)) completed++;
        if (!string.IsNullOrWhiteSpace(supplier.Country)) completed++;
        if (!string.IsNullOrWhiteSpace(supplier.ProductCategory)) completed++;
        if (!string.IsNullOrWhiteSpace(supplier.CompanyWebsite)) completed++;
        if (!string.IsNullOrWhiteSpace(supplier.Certifications)) completed++;
        if (supplier.Products != null && supplier.Products.Length > 0) completed++;
        
        return (completed * 100) / total;
    }
}

// Data models for dashboard
public class BuyerDashboardData
{
    public string CompanyName { get; set; } = "";
    public int ActiveRfqs { get; set; }
    public int PendingQuotes { get; set; }
    public int ActiveOrders { get; set; }
    public decimal TotalOrderValue { get; set; }
}

public class SupplierDashboardData
{
    public string CompanyName { get; set; } = "";
    public int ActiveProducts { get; set; }
    public int PendingRfqs { get; set; }
    public int ActiveOrders { get; set; }
    public int ProfileCompletion { get; set; }
}

public class AdminDashboardData
{
    public int TotalUsers { get; set; }
    public int TotalSuppliers { get; set; }
    public int TotalBuyers { get; set; }
    public int TotalProducts { get; set; }
    public int ActiveOrders { get; set; }
    public int PendingRfqs { get; set; }
}
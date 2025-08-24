using System.Collections.Generic;
using System.Threading.Tasks;
using FoodX.Core.Models.Entities;

namespace FoodX.Core.Repositories
{
    /// <summary>
    /// Product-specific repository interface
    /// </summary>
    public interface IProductRepository : IGenericRepository<Product>
    {
        // Product-specific methods
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category);
        Task<IEnumerable<Product>> GetProductsBySupplierAsync(int supplierId);
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        Task<Product?> GetProductBySkuAsync(string sku);
        Task<IEnumerable<Product>> GetProductsInPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<Product>> GetOrganicProductsAsync();
        Task<IEnumerable<Product>> GetProductsByOriginAsync(string origin);
        Task<bool> IsSkuUniqueAsync(string sku, int? excludeProductId = null);
        Task<Dictionary<string, int>> GetProductCountByCategoryAsync();
    }
}
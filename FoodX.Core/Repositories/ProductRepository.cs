using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FoodX.Core.Data;
using FoodX.Core.Models.Entities;

namespace FoodX.Core.Repositories
{
    /// <summary>
    /// Product repository implementation with business-specific methods
    /// </summary>
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(FoodXBusinessContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
        {
            return await _dbSet
                .Where(p => p.Category == category && p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsBySupplierAsync(int supplierId)
        {
            return await _dbSet
                .Where(p => p.SupplierId == supplierId)
                .Include(p => p.Supplier)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _dbSet
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveProductsAsync();

            var lowerSearchTerm = searchTerm.ToLower();
            
            return await _dbSet
                .Where(p => p.IsActive && (
                    p.Name.ToLower().Contains(lowerSearchTerm) ||
                    p.Description!.ToLower().Contains(lowerSearchTerm) ||
                    p.Category!.ToLower().Contains(lowerSearchTerm) ||
                    p.SKU!.ToLower().Contains(lowerSearchTerm)
                ))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Product?> GetProductBySkuAsync(string sku)
        {
            return await _dbSet
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.SKU == sku);
        }

        public async Task<IEnumerable<Product>> GetProductsInPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _dbSet
                .Where(p => p.IsActive && p.Price >= minPrice && p.Price <= maxPrice)
                .OrderBy(p => p.Price)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetOrganicProductsAsync()
        {
            return await _dbSet
                .Where(p => p.IsActive && p.IsOrganic == true)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByOriginAsync(string origin)
        {
            return await _dbSet
                .Where(p => p.IsActive && p.Origin == origin)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<bool> IsSkuUniqueAsync(string sku, int? excludeProductId = null)
        {
            var query = _dbSet.Where(p => p.SKU == sku);
            
            if (excludeProductId.HasValue)
            {
                query = query.Where(p => p.Id != excludeProductId.Value);
            }
            
            return !await query.AnyAsync();
        }

        public async Task<Dictionary<string, int>> GetProductCountByCategoryAsync()
        {
            return await _dbSet
                .Where(p => p.IsActive)
                .GroupBy(p => p.Category ?? "Uncategorized")
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count);
        }
    }
}
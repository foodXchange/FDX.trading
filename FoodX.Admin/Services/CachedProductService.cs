using Microsoft.EntityFrameworkCore;
using FoodX.Admin.Data;
using FoodX.Admin.Models;
using FoodX.Core.Services;
using System.Security.Cryptography;
using System.Text;

namespace FoodX.Admin.Services
{
    /// <summary>
    /// Service for invalidating cached data when changes occur
    /// </summary>
    public interface ICacheInvalidationService
    {
        Task InvalidateProductCacheAsync(int productId);
        Task InvalidateSupplierCacheAsync(int supplierId);
        Task InvalidateBuyerCacheAsync(int buyerId);
        Task InvalidateUserCacheAsync(string userId);
        Task InvalidateCompanyCacheAsync(int companyId);
        Task InvalidateSearchCacheAsync();
    }

    public class CacheInvalidationService : ICacheInvalidationService
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<CacheInvalidationService> _logger;

        public CacheInvalidationService(ICacheService cacheService, ILogger<CacheInvalidationService> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task InvalidateProductCacheAsync(int productId)
        {
            var key = GetProductKey(productId);
            await _cacheService.RemoveAsync(key);
            await InvalidateSearchCacheAsync();
            _logger.LogInformation("Invalidated product cache for ID: {ProductId}", productId);
        }

        public async Task InvalidateSupplierCacheAsync(int supplierId)
        {
            var key = GetSupplierKey(supplierId);
            await _cacheService.RemoveAsync(key);
            await InvalidateSearchCacheAsync();
            _logger.LogInformation("Invalidated supplier cache for ID: {SupplierId}", supplierId);
        }

        public async Task InvalidateBuyerCacheAsync(int buyerId)
        {
            var key = GetBuyerKey(buyerId);
            await _cacheService.RemoveAsync(key);
            _logger.LogInformation("Invalidated buyer cache for ID: {BuyerId}", buyerId);
        }

        public async Task InvalidateUserCacheAsync(string userId)
        {
            var key = GetUserKey(userId);
            await _cacheService.RemoveAsync(key);
            _logger.LogInformation("Invalidated user cache for ID: {UserId}", userId);
        }

        public async Task InvalidateCompanyCacheAsync(int companyId)
        {
            var key = GetCompanyKey(companyId);
            await _cacheService.RemoveAsync(key);
            _logger.LogInformation("Invalidated company cache for ID: {CompanyId}", companyId);
        }

        public async Task InvalidateSearchCacheAsync()
        {
            await _cacheService.RemoveByPrefixAsync("products:search:");
            _logger.LogInformation("Invalidated all search caches");
        }

        // Helper methods for cache keys
        public static string GetProductKey(int productId) => $"product:{productId}";
        public static string GetSupplierKey(int supplierId) => $"supplier:{supplierId}";
        public static string GetBuyerKey(int buyerId) => $"buyer:{buyerId}";
        public static string GetUserKey(string userId) => $"user:{userId}";
        public static string GetCompanyKey(int companyId) => $"company:{companyId}";
    }

    /// <summary>
    /// Service for cached product operations with automatic cache management
    /// </summary>
    public interface ICachedProductService
    {
        Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<Product>> GetProductsAsync(int page, int pageSize, string? category = null, CancellationToken cancellationToken = default);
        Task<List<Product>> SearchProductsAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<Product> CreateProductAsync(Product product, CancellationToken cancellationToken = default);
        Task<Product> UpdateProductAsync(Product product, CancellationToken cancellationToken = default);
        Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default);
        Task<List<Product>> GetSupplierProductsAsync(int supplierId, CancellationToken cancellationToken = default);
        Task<List<Product>> GetCompanyProductsAsync(int companyId, CancellationToken cancellationToken = default);
    }

    public class CachedProductService : ICachedProductService
    {
        private readonly FoodXDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly ICacheInvalidationService _cacheInvalidation;
        private readonly ILogger<CachedProductService> _logger;

        // Cache expiration times - optimized for performance
        private static readonly TimeSpan ProductCacheExpiration = TimeSpan.FromHours(1); // Increased for better performance
        private static readonly TimeSpan ListCacheExpiration = TimeSpan.FromMinutes(30); // Increased
        private static readonly TimeSpan SearchCacheExpiration = TimeSpan.FromMinutes(15); // Increased

        public CachedProductService(
            FoodXDbContext context,
            ICacheService cacheService,
            ICacheInvalidationService cacheInvalidation,
            ILogger<CachedProductService> logger)
        {
            _context = context;
            _cacheService = cacheService;
            _cacheInvalidation = cacheInvalidation;
            _logger = logger;
        }

        public async Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var cacheKey = CacheInvalidationService.GetProductKey(id);

            // Try to get from cache first
            var cached = await _cacheService.GetAsync<Product>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            // If not in cache, fetch from database
            _logger.LogDebug("Fetching product {ProductId} from database", id);
            
            var product = await _context.Products
                .Include(p => p.Supplier)
                .Include(p => p.Company)
                .AsSplitQuery() // Optimize for multiple includes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            // Cache the result if found
            if (product != null)
            {
                await _cacheService.SetAsync(cacheKey, product, ProductCacheExpiration);
            }

            return product;
        }

        public async Task<List<Product>> GetProductsAsync(int page, int pageSize, string? category = null, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"products:list:{page}:{pageSize}:{category ?? "all"}";

            // Try to get from cache first
            var cached = await _cacheService.GetAsync<List<Product>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            // If not in cache, fetch from database
            _logger.LogDebug("Fetching products page {Page} from database", page);

            var query = _context.Products
                .Include(p => p.Supplier)
                .Include(p => p.Company)
                .Where(p => p.IsAvailable)
                .AsSplitQuery() // Optimize for multiple includes
                .AsNoTracking();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            var products = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            // Cache the result
            await _cacheService.SetAsync(cacheKey, products, ListCacheExpiration);

            return products;
        }

        public async Task<List<Product>> SearchProductsAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            // Create a hash of the search query for the cache key
            var queryHash = ComputeHash(query.ToLower());
            var cacheKey = $"products:search:{queryHash}:{page}:{pageSize}";

            // Try to get from cache first
            var cached = await _cacheService.GetAsync<List<Product>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            // If not in cache, fetch from database
            _logger.LogDebug("Searching products with query '{Query}'", query);

            var searchQuery = query.ToLower();
            
            var products = await _context.Products
                .Include(p => p.Supplier)
                .Include(p => p.Company)
                .Where(p => p.IsAvailable &&
                    (EF.Functions.Like(p.Name, $"%{searchQuery}%") ||
                     (p.Description != null && EF.Functions.Like(p.Description, $"%{searchQuery}%")) ||
                     EF.Functions.Like(p.Category, $"%{searchQuery}%"))) // Use EF.Functions for better SQL
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsSplitQuery() // Optimize for multiple includes
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Cache the result
            await _cacheService.SetAsync(cacheKey, products, SearchCacheExpiration);

            return products;
        }

        public async Task<Product> CreateProductAsync(Product product, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new product: {ProductName}", product.Name);

            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate related caches
            await _cacheInvalidation.InvalidateSearchCacheAsync();
            if (product.SupplierId.HasValue)
            {
                await InvalidateSupplierProductsCacheAsync(product.SupplierId.Value);
            }
            if (product.CompanyId.HasValue)
            {
                await InvalidateCompanyProductsCacheAsync(product.CompanyId.Value);
            }

            // Cache the new product
            var cacheKey = CacheInvalidationService.GetProductKey(product.Id);
            await _cacheService.SetAsync(cacheKey, product, ProductCacheExpiration);

            return product;
        }

        public async Task<Product> UpdateProductAsync(Product product, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating product: {ProductId}", product.Id);

            var existingProduct = await _context.Products.FindAsync(new object[] { product.Id }, cancellationToken);
            if (existingProduct == null)
            {
                throw new ArgumentException($"Product with ID {product.Id} not found");
            }

            // Track old values for cache invalidation
            var oldSupplierId = existingProduct.SupplierId;
            var oldCompanyId = existingProduct.CompanyId;

            // Update properties
            _context.Entry(existingProduct).CurrentValues.SetValues(product);
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate caches
            await _cacheInvalidation.InvalidateProductCacheAsync(product.Id);
            
            // Invalidate old and new supplier/company caches if changed
            if (oldSupplierId != product.SupplierId)
            {
                if (oldSupplierId.HasValue)
                    await InvalidateSupplierProductsCacheAsync(oldSupplierId.Value);
                if (product.SupplierId.HasValue)
                    await InvalidateSupplierProductsCacheAsync(product.SupplierId.Value);
            }

            if (oldCompanyId != product.CompanyId)
            {
                if (oldCompanyId.HasValue)
                    await InvalidateCompanyProductsCacheAsync(oldCompanyId.Value);
                if (product.CompanyId.HasValue)
                    await InvalidateCompanyProductsCacheAsync(product.CompanyId.Value);
            }

            return existingProduct;
        }

        public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting product: {ProductId}", id);

            var product = await _context.Products.FindAsync(new object[] { id }, cancellationToken);
            if (product == null)
            {
                return false;
            }

            // Soft delete by marking as unavailable
            product.IsAvailable = false;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate caches
            await _cacheInvalidation.InvalidateProductCacheAsync(id);
            if (product.SupplierId.HasValue)
            {
                await InvalidateSupplierProductsCacheAsync(product.SupplierId.Value);
            }
            if (product.CompanyId.HasValue)
            {
                await InvalidateCompanyProductsCacheAsync(product.CompanyId.Value);
            }

            return true;
        }

        public async Task<List<Product>> GetSupplierProductsAsync(int supplierId, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"products:supplier:{supplierId}";

            // Try to get from cache first
            var cached = await _cacheService.GetAsync<List<Product>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            // If not in cache, fetch from database
            _logger.LogDebug("Fetching products for supplier {SupplierId}", supplierId);

            var products = await _context.Products
                .Include(p => p.Company)
                .Where(p => p.SupplierId == supplierId && p.IsAvailable)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Cache the result
            await _cacheService.SetAsync(cacheKey, products, ListCacheExpiration);

            return products;
        }

        public async Task<List<Product>> GetCompanyProductsAsync(int companyId, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"products:company:{companyId}";

            // Try to get from cache first
            var cached = await _cacheService.GetAsync<List<Product>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            // If not in cache, fetch from database
            _logger.LogDebug("Fetching products for company {CompanyId}", companyId);

            var products = await _context.Products
                .Include(p => p.Supplier)
                .Where(p => p.CompanyId == companyId && p.IsAvailable)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Cache the result
            await _cacheService.SetAsync(cacheKey, products, ListCacheExpiration);

            return products;
        }

        private async Task InvalidateSupplierProductsCacheAsync(int supplierId)
        {
            var cacheKey = $"products:supplier:{supplierId}";
            await _cacheService.RemoveAsync(cacheKey);
        }

        private async Task InvalidateCompanyProductsCacheAsync(int companyId)
        {
            var cacheKey = $"products:company:{companyId}";
            await _cacheService.RemoveAsync(cacheKey);
        }

        private static string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hashBytes).Replace("/", "_").Replace("+", "-").Substring(0, 8);
        }
    }
}
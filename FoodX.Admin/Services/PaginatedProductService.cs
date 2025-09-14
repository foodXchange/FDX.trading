using FoodX.Admin.Data;
using FoodX.Admin.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodX.Admin.Services
{
    public class PaginatedProductService : IPaginatedProductService
    {
        private readonly IDbContextFactory<FoodXDbContext> _contextFactory;
        private readonly IMemoryCache _cache;
        private const string CATEGORIES_CACHE_KEY = "product_categories";
        private const string PRODUCT_COUNT_CACHE_KEY = "product_total_count";

        public PaginatedProductService(
            IDbContextFactory<FoodXDbContext> contextFactory,
            IMemoryCache cache)
        {
            _contextFactory = contextFactory;
            _cache = cache;
        }

        public async Task<PaginatedResult<Product>> GetProductsAsync(
            int page = 1,
            int pageSize = 50,
            string? searchTerm = null,
            string? category = null,
            string? sortBy = null,
            bool sortDescending = false)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // Start with base query
            IQueryable<Product> query = context.Products
                .Include(p => p.Supplier)
                    .ThenInclude(s => s.User)
                .Include(p => p.Supplier)
                    .ThenInclude(s => s.Company)
                .AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
                    (p.SKU != null && p.SKU.ToLower().Contains(searchTerm)) ||
                    (p.Supplier != null && p.Supplier.User != null && p.Supplier.User.CompanyName != null && p.Supplier.User.CompanyName.ToLower().Contains(searchTerm)));
            }

            // Apply category filter
            if (!string.IsNullOrWhiteSpace(category) && category != "All Categories")
            {
                query = query.Where(p => p.Category == category);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "name" => sortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "price" => sortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "category" => sortDescending ? query.OrderByDescending(p => p.Category) : query.OrderBy(p => p.Category),
                "stock" => sortDescending ? query.OrderByDescending(p => p.StockQuantity) : query.OrderBy(p => p.StockQuantity),
                "createdat" => sortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt) // Default sort by newest first
            };

            // Apply pagination
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Product>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            // Try to get from cache first
            if (_cache.TryGetValue(CATEGORIES_CACHE_KEY, out List<string>? cachedCategories))
            {
                return cachedCategories ?? new List<string>();
            }

            using var context = await _contextFactory.CreateDbContextAsync();

            var categories = await context.Products
                .Where(p => !string.IsNullOrEmpty(p.Category))
                .Select(p => p.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // Add "All Categories" at the beginning
            categories.Insert(0, "All Categories");

            // Cache for 5 minutes
            _cache.Set(CATEGORIES_CACHE_KEY, categories, TimeSpan.FromMinutes(5));

            return categories;
        }

        public async Task<int> GetTotalProductsAsync()
        {
            // Try to get from cache first
            if (_cache.TryGetValue(PRODUCT_COUNT_CACHE_KEY, out int cachedCount))
            {
                return cachedCount;
            }

            using var context = await _contextFactory.CreateDbContextAsync();
            var count = await context.Products.CountAsync();

            // Cache for 1 minute
            _cache.Set(PRODUCT_COUNT_CACHE_KEY, count, TimeSpan.FromMinutes(1));

            return count;
        }

        public async Task<int> GetProductCountAsync(string? searchTerm = null, string? category = null)
        {
            // Alias for GetTotalProductsAsync
            return await GetTotalProductsAsync();
        }

        public void InvalidateCache()
        {
            _cache.Remove(CATEGORIES_CACHE_KEY);
            _cache.Remove(PRODUCT_COUNT_CACHE_KEY);
        }
    }
}
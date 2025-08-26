using FoodX.Admin.Data;
using FoodX.Admin.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;

namespace FoodX.Admin.Services
{
    public class SupplierSearchService
    {
        private readonly FoodXDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SupplierSearchService> _logger;

        public SupplierSearchService(
            FoodXDbContext context,
            IMemoryCache cache,
            ILogger<SupplierSearchService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public class SearchParameters
        {
            public string? SearchTerm { get; set; }
            public string? Country { get; set; }
            public string? Category { get; set; }
            public bool ShowOnlyWithProducts { get; set; }
            public bool FilterKosher { get; set; }
            public bool FilterHalal { get; set; }
            public bool FilterOrganic { get; set; }
            public bool FilterGlutenFree { get; set; }
            public int PageNumber { get; set; } = 0;
            public int PageSize { get; set; } = 24;
        }

        public class SearchResult
        {
            public List<FoodXSupplier> Suppliers { get; set; } = new();
            public int TotalCount { get; set; }
            public bool HasMore { get; set; }
        }

        public async Task<SearchResult> SearchSuppliersAsync(SearchParameters parameters)
        {
            try
            {
                // Build cache key
                var cacheKey = $"suppliers_{parameters.SearchTerm}_{parameters.Country}_{parameters.Category}_{parameters.ShowOnlyWithProducts}_{parameters.FilterKosher}_{parameters.FilterHalal}_{parameters.FilterOrganic}_{parameters.FilterGlutenFree}_{parameters.PageNumber}_{parameters.PageSize}";

                // Try to get from cache
                if (_cache.TryGetValue(cacheKey, out SearchResult? cachedResult) && cachedResult != null)
                {
                    _logger.LogDebug("Returning cached supplier search results");
                    return cachedResult;
                }

                // Build query
                var query = BuildSearchQuery(parameters);

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination and projection
                var suppliers = await query
                    .OrderBy(s => s.SupplierName)
                    .Skip(parameters.PageNumber * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .Select(s => new FoodXSupplier
                    {
                        Id = s.Id,
                        SupplierName = s.SupplierName,
                        Country = s.Country,
                        ProductCategory = s.ProductCategory,
                        Products = s.Products,
                        CompanyEmail = s.CompanyEmail,
                        Description = s.Description,
                        CompanyWebsite = s.CompanyWebsite,
                        Phone = s.Phone,
                        KosherCertification = s.KosherCertification,
                        Certifications = s.Certifications
                    })
                    .ToListAsync();

                var result = new SearchResult
                {
                    Suppliers = suppliers,
                    TotalCount = totalCount,
                    HasMore = (parameters.PageNumber + 1) * parameters.PageSize < totalCount
                };

                // Cache the result for 5 minutes
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching suppliers");
                throw;
            }
        }

        private IQueryable<FoodXSupplier> BuildSearchQuery(SearchParameters parameters)
        {
            var query = _context.FoodXSuppliers.AsNoTracking();

            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                var term = parameters.SearchTerm.Trim();
                query = query.Where(s =>
                    EF.Functions.Like(s.SupplierName, $"%{term}%") ||
                    (s.ProductCategory != null && EF.Functions.Like(s.ProductCategory, $"%{term}%")) ||
                    (s.Products != null && EF.Functions.Like(s.Products, $"%{term}%")) ||
                    (s.Description != null && EF.Functions.Like(s.Description, $"%{term}%")));
            }

            // Apply country filter
            if (!string.IsNullOrEmpty(parameters.Country))
            {
                query = query.Where(s => s.Country == parameters.Country);
            }

            // Apply category filter
            if (!string.IsNullOrEmpty(parameters.Category))
            {
                query = query.Where(s => s.ProductCategory == parameters.Category);
            }

            // Apply products filter
            if (parameters.ShowOnlyWithProducts)
            {
                query = query.Where(s => s.Products != null && s.Products != "");
            }

            // Apply certification filters using optimized boolean columns
            if (parameters.FilterKosher)
            {
                query = query.Where(s => s.IsKosherCertified == true);
            }

            if (parameters.FilterHalal)
            {
                query = query.Where(s => s.IsHalalCertified == true);
            }

            if (parameters.FilterOrganic)
            {
                query = query.Where(s => s.IsOrganicCertified == true);
            }

            if (parameters.FilterGlutenFree)
            {
                query = query.Where(s => s.IsGlutenFreeCertified == true);
            }

            return query;
        }

        public async Task<Dictionary<string, int>> GetSupplierStatisticsAsync()
        {
            const string cacheKey = "supplier_statistics";

            if (_cache.TryGetValue(cacheKey, out Dictionary<string, int>? stats) && stats != null)
            {
                return stats;
            }

            var statistics = new Dictionary<string, int>
            {
                ["TotalSuppliers"] = await _context.FoodXSuppliers.CountAsync(),
                ["WithProducts"] = await _context.FoodXSuppliers.CountAsync(s => s.Products != null && s.Products != ""),
                ["WithEmail"] = await _context.FoodXSuppliers.CountAsync(s => s.CompanyEmail != null && s.CompanyEmail != ""),
                ["Verified"] = await _context.FoodXSuppliers.CountAsync(s => s.IsVerified == true)
            };

            _cache.Set(cacheKey, statistics, TimeSpan.FromHours(1));
            return statistics;
        }

        public async Task<List<string>> GetTopCountriesAsync(int count = 20)
        {
            const string cacheKey = "top_countries";

            if (_cache.TryGetValue(cacheKey, out List<string>? countries) && countries != null)
            {
                return countries;
            }

            var topCountries = await _context.FoodXSuppliers
                .Where(s => !string.IsNullOrEmpty(s.Country))
                .GroupBy(s => s.Country!)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.Key)
                .ToListAsync();

            _cache.Set(cacheKey, topCountries, TimeSpan.FromHours(6));
            return topCountries;
        }

        public async Task<List<string>> GetTopCategoriesAsync(int count = 30)
        {
            const string cacheKey = "top_categories";

            if (_cache.TryGetValue(cacheKey, out List<string>? categories) && categories != null)
            {
                return categories;
            }

            var topCategories = await _context.FoodXSuppliers
                .Where(s => !string.IsNullOrEmpty(s.ProductCategory))
                .Select(s => s.ProductCategory!)
                .Distinct()
                .OrderBy(c => c)
                .Take(count)
                .ToListAsync();

            _cache.Set(cacheKey, topCategories, TimeSpan.FromHours(6));
            return topCategories;
        }

        public void ClearCache()
        {
            if (_cache is MemoryCache memoryCache)
            {
                memoryCache.Compact(1.0);
            }
        }
    }
}
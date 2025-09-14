using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using FoodX.Admin.Data;
using FoodX.Admin.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FoodX.Admin.Services
{
    public interface IAISearchService
    {
        Task<AISearchResult> SearchAsync(string query, SearchOptions? options = null);
        Task<List<string>> GetSearchSuggestionsAsync(string query);
        Task<SearchInsights> AnalyzeSearchIntentAsync(string query);
        Task SaveSearchHistoryAsync(string userId, string query, AISearchResult result);
    }

    public class AISearchService : IAISearchService
    {
        private readonly IDbContextFactory<FoodXDbContext> _contextFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AISearchService> _logger;
        private readonly IConfiguration _configuration;
        private readonly AzureOpenAIClient? _openAIClient;

        public AISearchService(
            IDbContextFactory<FoodXDbContext> contextFactory,
            IMemoryCache cache,
            ILogger<AISearchService> logger,
            IConfiguration configuration)
        {
            _contextFactory = contextFactory;
            _cache = cache;
            _logger = logger;
            _configuration = configuration;

            // Initialize OpenAI client if configured
            var apiKey = configuration["AzureOpenAI:ApiKey"];
            var endpoint = configuration["AzureOpenAI:Endpoint"];

            if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(endpoint))
            {
                _openAIClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            }
        }

        public async Task<AISearchResult> SearchAsync(string query, SearchOptions? options = null)
        {
            try
            {
                // Check cache first
                var cacheKey = $"ai_search_{query}_{JsonSerializer.Serialize(options)}";
                if (_cache.TryGetValue(cacheKey, out AISearchResult? cachedResult))
                {
                    return cachedResult!;
                }

                var result = new AISearchResult
                {
                    Query = query,
                    SearchedAt = DateTime.UtcNow
                };

                // Analyze search intent using AI
                var insights = await AnalyzeSearchIntentAsync(query);
                result.Intent = insights;

                using var context = await _contextFactory.CreateDbContextAsync();

                // Search products based on intent
                var productsQuery = context.Products
                    .Include(p => p.Supplier)
                        .ThenInclude(s => s.User)
                    .Include(p => p.Supplier)
                        .ThenInclude(s => s.Company)
                    .AsNoTracking();

                // Apply natural language filters
                if (insights.Categories?.Any() == true)
                {
                    productsQuery = productsQuery.Where(p => insights.Categories.Contains(p.Category));
                }

                if (insights.PriceRange != null)
                {
                    if (insights.PriceRange.Min.HasValue)
                        productsQuery = productsQuery.Where(p => p.Price >= insights.PriceRange.Min.Value);
                    if (insights.PriceRange.Max.HasValue)
                        productsQuery = productsQuery.Where(p => p.Price <= insights.PriceRange.Max.Value);
                }

                // Text search
                var searchTerms = ExtractSearchTerms(query);
                foreach (var term in searchTerms)
                {
                    var searchTerm = term.ToLower();
                    productsQuery = productsQuery.Where(p =>
                        p.Name.ToLower().Contains(searchTerm) ||
                        (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
                        (p.Category != null && p.Category.ToLower().Contains(searchTerm)));
                }

                // Apply pagination
                var products = await productsQuery
                    .Take(options?.MaxResults ?? 20)
                    .Select(p => new ProductSearchResult
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Category = p.Category,
                        Description = p.Description,
                        Price = p.Price,
                        Unit = p.Unit,
                        ImageUrl = p.ImageUrl,
                        SupplierName = p.Supplier != null && p.Supplier.User != null && p.Supplier.User.CompanyName != null ? p.Supplier.User.CompanyName :
                                      p.Supplier != null && p.Supplier.Company != null ? p.Supplier.Company.Name :
                                      p.Supplier != null && p.Supplier.User != null ? p.Supplier.User.FirstName + " " + p.Supplier.User.LastName : null,
                        Relevance = CalculateRelevance(p, query)
                    })
                    .OrderByDescending(p => p.Relevance)
                    .ToListAsync();

                result.Products = products;

                // Search suppliers
                var suppliers = await SearchSuppliersAsync(context, query, insights);
                result.Suppliers = suppliers;

                // Generate AI recommendations
                if (_openAIClient != null && products.Any())
                {
                    result.AIRecommendations = await GenerateRecommendationsAsync(query, products);
                }

                // Cache the result
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing AI search for query: {Query}", query);
                throw;
            }
        }

        public async Task<List<string>> GetSearchSuggestionsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return new List<string>();

            var cacheKey = $"suggestions_{query}";
            if (_cache.TryGetValue(cacheKey, out List<string>? cached))
                return cached!;

            using var context = await _contextFactory.CreateDbContextAsync();

            // Get product name suggestions
            var productSuggestions = await context.Products
                .Where(p => p.Name.Contains(query))
                .Select(p => p.Name)
                .Distinct()
                .Take(5)
                .ToListAsync();

            // Get category suggestions
            var categorySuggestions = await context.Products
                .Where(p => p.Category != null && p.Category.Contains(query))
                .Select(p => p.Category!)
                .Distinct()
                .Take(3)
                .ToListAsync();

            var suggestions = productSuggestions
                .Concat(categorySuggestions)
                .Distinct()
                .Take(8)
                .ToList();

            _cache.Set(cacheKey, suggestions, TimeSpan.FromMinutes(10));

            return suggestions;
        }

        public async Task<SearchInsights> AnalyzeSearchIntentAsync(string query)
        {
            var insights = new SearchInsights
            {
                OriginalQuery = query
            };

            // Extract categories from query
            insights.Categories = ExtractCategories(query);

            // Extract price range
            insights.PriceRange = ExtractPriceRange(query);

            // Extract quantities
            insights.Quantities = ExtractQuantities(query);

            // Determine search type
            insights.SearchType = DetermineSearchType(query);

            // Extract product attributes
            insights.Attributes = ExtractAttributes(query);

            // Use AI for advanced intent analysis if available
            if (_openAIClient != null)
            {
                try
                {
                    var prompt = $"Analyze this product search query and extract: categories, price expectations, quantities, and key requirements: '{query}'";

                    // This would use the OpenAI API to get more sophisticated intent analysis
                    // For now, we'll use rule-based extraction
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "AI intent analysis failed, using fallback");
                }
            }

            return await Task.FromResult(insights);
        }

        public async Task SaveSearchHistoryAsync(string userId, string query, AISearchResult result)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var history = new SearchHistory
                {
                    UserId = userId,
                    Query = query,
                    ResultCount = result.Products?.Count ?? 0,
                    SearchedAt = DateTime.UtcNow,
                    Categories = string.Join(",", result.Intent?.Categories ?? new List<string>()),
                    IsSuccessful = result.Products?.Any() == true
                };

                context.Set<SearchHistory>().Add(history);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving search history");
            }
        }

        private async Task<List<SupplierSearchResult>> SearchSuppliersAsync(
            FoodXDbContext context, string query, SearchInsights insights)
        {
            var suppliersQuery = context.Set<FoodXSupplier>()
                .AsNoTracking();

            // Filter by categories if identified
            if (insights.Categories?.Any() == true)
            {
                suppliersQuery = suppliersQuery.Where(s =>
                    s.Categories != null && insights.Categories.Any(c => s.Categories.Contains(c)));
            }

            var suppliers = await suppliersQuery
                .Where(s => s.SupplierName.Contains(query) ||
                           (s.ContactPerson != null && s.ContactPerson.Contains(query)) ||
                           (s.Description != null && s.Description.Contains(query)) ||
                           (s.Categories != null && s.Categories.Contains(query)))
                .Take(10)
                .Select(s => new SupplierSearchResult
                {
                    Id = s.Id,
                    Name = s.SupplierName,
                    Country = s.Country,
                    Categories = s.Categories,
                    Rating = s.Rating ?? 4.5m,
                    MatchScore = CalculateSupplierMatch(s, query)
                })
                .OrderByDescending(s => s.MatchScore)
                .ToListAsync();

            return suppliers;
        }

        private List<string> ExtractSearchTerms(string query)
        {
            // Simple tokenization - can be enhanced with NLP
            return query.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(t => t.Length > 2)
                .ToList();
        }

        private List<string> ExtractCategories(string query)
        {
            var categories = new List<string>();
            var knownCategories = new[] {
                "oils", "dairy", "meat", "seafood", "grains", "fruits",
                "vegetables", "beverages", "snacks", "frozen", "organic"
            };

            var lowerQuery = query.ToLower();
            foreach (var category in knownCategories)
            {
                if (lowerQuery.Contains(category))
                {
                    categories.Add(category);
                }
            }

            return categories;
        }

        private PriceRange? ExtractPriceRange(string query)
        {
            // Simple price extraction - can be enhanced with regex
            var priceRange = new PriceRange();

            if (query.Contains("under"))
            {
                // Extract max price
                var parts = query.Split("under");
                if (parts.Length > 1)
                {
                    var priceStr = System.Text.RegularExpressions.Regex.Match(parts[1], @"\d+").Value;
                    if (decimal.TryParse(priceStr, out var price))
                    {
                        priceRange.Max = price;
                    }
                }
            }

            if (query.Contains("above") || query.Contains("over"))
            {
                // Extract min price
                var parts = query.Split(new[] { "above", "over" }, StringSplitOptions.None);
                if (parts.Length > 1)
                {
                    var priceStr = System.Text.RegularExpressions.Regex.Match(parts[1], @"\d+").Value;
                    if (decimal.TryParse(priceStr, out var price))
                    {
                        priceRange.Min = price;
                    }
                }
            }

            return priceRange.Min.HasValue || priceRange.Max.HasValue ? priceRange : null;
        }

        private List<int> ExtractQuantities(string query)
        {
            var quantities = new List<int>();
            var matches = System.Text.RegularExpressions.Regex.Matches(query, @"\d+");

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (int.TryParse(match.Value, out var qty))
                {
                    quantities.Add(qty);
                }
            }

            return quantities;
        }

        private SearchType DetermineSearchType(string query)
        {
            var lower = query.ToLower();

            if (lower.Contains("supplier") || lower.Contains("manufacturer"))
                return SearchType.Supplier;

            if (lower.Contains("cheap") || lower.Contains("budget") || lower.Contains("affordable"))
                return SearchType.PriceFocused;

            if (lower.Contains("organic") || lower.Contains("natural") || lower.Contains("eco"))
                return SearchType.QualityFocused;

            if (lower.Contains("bulk") || lower.Contains("wholesale"))
                return SearchType.BulkOrder;

            return SearchType.General;
        }

        private Dictionary<string, string> ExtractAttributes(string query)
        {
            var attributes = new Dictionary<string, string>();

            if (query.ToLower().Contains("organic"))
                attributes["organic"] = "true";

            if (query.ToLower().Contains("halal"))
                attributes["halal"] = "true";

            if (query.ToLower().Contains("kosher"))
                attributes["kosher"] = "true";

            if (query.ToLower().Contains("gluten-free"))
                attributes["glutenFree"] = "true";

            return attributes;
        }

        private double CalculateRelevance(Product product, string query)
        {
            double relevance = 0;
            var lowerQuery = query.ToLower();
            var terms = ExtractSearchTerms(query);

            // Name match (highest weight)
            if (product.Name.ToLower().Contains(lowerQuery))
                relevance += 10;

            // Category match
            if (product.Category?.ToLower().Contains(lowerQuery) == true)
                relevance += 5;

            // Description match
            if (product.Description?.ToLower().Contains(lowerQuery) == true)
                relevance += 3;

            // Individual term matches
            foreach (var term in terms)
            {
                if (product.Name.ToLower().Contains(term.ToLower()))
                    relevance += 2;
            }

            return relevance;
        }

        private double CalculateSupplierMatch(FoodXSupplier supplier, string query)
        {
            double score = 0;
            var lowerQuery = query.ToLower();

            if (supplier.SupplierName.ToLower().Contains(lowerQuery))
                score += 10;

            if (supplier.ContactPerson?.ToLower().Contains(lowerQuery) == true)
                score += 8;

            if (supplier.Categories?.ToLower().Contains(lowerQuery) == true)
                score += 5;

            if (supplier.Description?.ToLower().Contains(lowerQuery) == true)
                score += 3;

            if (supplier.ProductCategory?.ToLower().Contains(lowerQuery) == true)
                score += 4;

            return score;
        }

        private async Task<string> GenerateRecommendationsAsync(string query, List<ProductSearchResult> products)
        {
            // This would use OpenAI to generate recommendations
            // For now, return a simple recommendation
            var topProduct = products.FirstOrDefault();
            if (topProduct != null)
            {
                return $"Based on your search for '{query}', we recommend {topProduct.Name} from {topProduct.SupplierName}. " +
                       $"This product offers excellent value at {topProduct.Price:C} per {topProduct.Unit}.";
            }

            return "Try refining your search with more specific terms or browse our categories.";
        }
    }

    // Supporting models
    public class AISearchResult
    {
        public string Query { get; set; } = "";
        public DateTime SearchedAt { get; set; }
        public List<ProductSearchResult> Products { get; set; } = new();
        public List<SupplierSearchResult> Suppliers { get; set; } = new();
        public SearchInsights? Intent { get; set; }
        public string? AIRecommendations { get; set; }
    }

    public class ProductSearchResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Category { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Unit { get; set; }
        public string? ImageUrl { get; set; }
        public string? SupplierName { get; set; }
        public double Relevance { get; set; }
    }

    public class SupplierSearchResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Country { get; set; }
        public string? Categories { get; set; }
        public decimal Rating { get; set; }
        public double MatchScore { get; set; }
    }

    public class SearchInsights
    {
        public string OriginalQuery { get; set; } = "";
        public List<string> Categories { get; set; } = new();
        public PriceRange? PriceRange { get; set; }
        public List<int> Quantities { get; set; } = new();
        public SearchType SearchType { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new();
    }

    public class PriceRange
    {
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }
    }

    public enum SearchType
    {
        General,
        Supplier,
        PriceFocused,
        QualityFocused,
        BulkOrder
    }

    public class SearchOptions
    {
        public int MaxResults { get; set; } = 20;
        public bool IncludeSuppliers { get; set; } = true;
        public bool IncludeRecommendations { get; set; } = true;
        public string? Category { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinPrice { get; set; }
    }

    public class SearchHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public string Query { get; set; } = "";
        public int ResultCount { get; set; }
        public DateTime SearchedAt { get; set; }
        public string? Categories { get; set; }
        public bool IsSuccessful { get; set; }
    }
}
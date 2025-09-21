using FoodX.Simple.Data;
using FoodX.Simple.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodX.Simple.Services
{
    public interface IProductService
    {
        Task<PagedResult<Product>> GetProductsAsync(int page = 1, int pageSize = 10, string? searchTerm = null, string? category = null);
        Task<Product?> GetProductByIdAsync(int id);
        Task<List<string>> GetCategoriesAsync();
        Task<Product> AddProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
        Task SeedInitialProductsAsync();
    }

    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ApplicationDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<Product>> GetProductsAsync(int page = 1, int pageSize = 10, string? searchTerm = null, string? category = null)
        {
            var query = _context.Products.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    p.Supplier.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm));
            }

            // Apply category filter
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }

            // Get total count for pagination
            var totalItems = await query.CountAsync();

            // Apply pagination
            var items = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Product>
            {
                Items = items,
                TotalItems = totalItems,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _context.Products
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<Product> AddProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SeedInitialProductsAsync()
        {
            // Check if products already exist
            if (await _context.Products.AnyAsync())
            {
                _logger.LogInformation("Products already exist in database");
                return;
            }

            var initialProducts = new List<Product>
            {
                new Product { Name = "Fresh Apples", Category = "Fruits", Supplier = "Green Valley Farms", Price = 2.99m, Unit = "kg", StockQuantity = 500, Description = "Premium quality red apples, perfect for retail." },
                new Product { Name = "Organic Bananas", Category = "Fruits", Supplier = "Tropical Harvest Co.", Price = 1.99m, Unit = "kg", StockQuantity = 750, Description = "Certified organic bananas from sustainable farms." },
                new Product { Name = "Fresh Tomatoes", Category = "Vegetables", Supplier = "Sun Garden Produce", Price = 3.49m, Unit = "kg", StockQuantity = 300, Description = "Vine-ripened tomatoes, ideal for restaurants." },
                new Product { Name = "Whole Milk", Category = "Dairy", Supplier = "Valley Dairy Farm", Price = 3.99m, Unit = "gallon", StockQuantity = 200, Description = "Fresh whole milk from grass-fed cows." },
                new Product { Name = "Chicken Breast", Category = "Meat", Supplier = "Prairie Poultry", Price = 8.99m, Unit = "kg", StockQuantity = 150, Description = "Premium quality boneless chicken breast." },
                new Product { Name = "Brown Rice", Category = "Grains", Supplier = "Golden Fields Co.", Price = 2.49m, Unit = "kg", StockQuantity = 1000, Description = "Nutritious whole grain brown rice." },
                new Product { Name = "Fresh Oranges", Category = "Fruits", Supplier = "Citrus Grove Farm", Price = 2.79m, Unit = "kg", StockQuantity = 400, Description = "Sweet and juicy oranges, perfect for juice." },
                new Product { Name = "Broccoli", Category = "Vegetables", Supplier = "Green Valley Farms", Price = 4.99m, Unit = "kg", StockQuantity = 250, Description = "Fresh organic broccoli crowns." },
                new Product { Name = "Greek Yogurt", Category = "Dairy", Supplier = "Mediterranean Dairy", Price = 5.49m, Unit = "kg", StockQuantity = 100, Description = "Authentic Greek yogurt, high in protein." },
                new Product { Name = "Ground Beef", Category = "Meat", Supplier = "Ranch Direct Meats", Price = 7.99m, Unit = "kg", StockQuantity = 200, Description = "Premium lean ground beef, 90/10 blend." },
                new Product { Name = "Fresh Strawberries", Category = "Fruits", Supplier = "Berry Best Farms", Price = 4.99m, Unit = "kg", StockQuantity = 300, Description = "Sweet, juicy strawberries picked at peak ripeness." },
                new Product { Name = "Carrots", Category = "Vegetables", Supplier = "Root Valley Produce", Price = 1.99m, Unit = "kg", StockQuantity = 600, Description = "Fresh organic carrots, perfect for juicing." },
                new Product { Name = "Cheddar Cheese", Category = "Dairy", Supplier = "Artisan Cheese Co.", Price = 9.99m, Unit = "kg", StockQuantity = 150, Description = "Aged sharp cheddar cheese." },
                new Product { Name = "Salmon Fillets", Category = "Meat", Supplier = "Ocean Fresh Seafood", Price = 15.99m, Unit = "kg", StockQuantity = 100, Description = "Wild-caught Atlantic salmon fillets." },
                new Product { Name = "Quinoa", Category = "Grains", Supplier = "Andean Harvest", Price = 5.99m, Unit = "kg", StockQuantity = 500, Description = "Organic quinoa, high in protein and nutrients." }
            };

            await _context.Products.AddRangeAsync(initialProducts);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Seeded {initialProducts.Count} products to database");
        }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
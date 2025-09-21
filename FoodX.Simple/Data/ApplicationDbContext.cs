using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FoodX.Simple.Models;

namespace FoodX.Simple.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<UploadedFile> UploadedFiles { get; set; }
        public DbSet<ImportedProduct> ImportedProducts { get; set; }
        public DbSet<ProductBrief> ProductBriefs { get; set; }
        public DbSet<RFQ> RFQs { get; set; }
        public DbSet<Project> Projects { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure entity relationships
            builder.Entity<RFQ>()
                .HasOne(r => r.ProductBrief)
                .WithOne(pb => pb.GeneratedRFQ)
                .HasForeignKey<RFQ>(r => r.ProductBriefId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Project>()
                .HasOne(p => p.RFQ)
                .WithMany()
                .HasForeignKey(p => p.RFQId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed initial products
            builder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Fresh Apples", Category = "Fruits", Supplier = "Green Valley Farms", Price = 2.99m, Unit = "kg", StockQuantity = 500, Description = "Premium quality red apples, perfect for retail." },
                new Product { Id = 2, Name = "Organic Bananas", Category = "Fruits", Supplier = "Tropical Harvest Co.", Price = 1.99m, Unit = "kg", StockQuantity = 750, Description = "Certified organic bananas from sustainable farms." },
                new Product { Id = 3, Name = "Fresh Tomatoes", Category = "Vegetables", Supplier = "Sun Garden Produce", Price = 3.49m, Unit = "kg", StockQuantity = 300, Description = "Vine-ripened tomatoes, ideal for restaurants." },
                new Product { Id = 4, Name = "Whole Milk", Category = "Dairy", Supplier = "Valley Dairy Farm", Price = 3.99m, Unit = "gallon", StockQuantity = 200, Description = "Fresh whole milk from grass-fed cows." },
                new Product { Id = 5, Name = "Chicken Breast", Category = "Meat", Supplier = "Prairie Poultry", Price = 8.99m, Unit = "kg", StockQuantity = 150, Description = "Premium quality boneless chicken breast." }
            );
        }
    }
}
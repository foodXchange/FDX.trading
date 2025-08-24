using Microsoft.EntityFrameworkCore;
using FoodX.Core.Models.Entities;

namespace FoodX.Core.Data;

public class FoodXCoreContext : DbContext
{
    public FoodXCoreContext(DbContextOptions<FoodXCoreContext> options) : base(options)
    {
    }

    public DbSet<Buyer> Buyers { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Exhibitor> Exhibitors { get; set; }
    public DbSet<Exhibition> Exhibitions { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entity relationships and constraints here
        modelBuilder.Entity<Exhibitor>()
            .HasOne(e => e.Exhibition)
            .WithMany(ex => ex.Exhibitors)
            .HasForeignKey(e => e.ExhibitionId);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Buyer)
            .WithMany()
            .HasForeignKey(o => o.BuyerId);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Supplier)
            .WithMany()
            .HasForeignKey(o => o.SupplierId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId);
    }
}
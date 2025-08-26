using Microsoft.EntityFrameworkCore;
using FoodX.Core.Models.Entities;
using FoodX.Core.Models;

namespace FoodX.Core.Data
{
    /// <summary>
    /// Main business context for FoodX platform
    /// Used by all portals for business data access
    /// </summary>
    public class FoodXBusinessContext : BaseDbContext
    {
        public FoodXBusinessContext(DbContextOptions<FoodXBusinessContext> options)
            : base(options)
        {
        }

        #region Core Entities (Shared across all portals)

        public DbSet<Company> Companies { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Buyer> Buyers { get; set; } = null!;
        public DbSet<Supplier> Suppliers { get; set; } = null!;
        public DbSet<Exhibitor> Exhibitors { get; set; } = null!;
        public DbSet<Exhibition> Exhibitions { get; set; } = null!;

        #endregion

        #region Trading Entities

        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<Quote> Quotes { get; set; } = null!;
        public DbSet<QuoteItem> QuoteItems { get; set; } = null!;
        public DbSet<RFQ> RFQs { get; set; } = null!;
        public DbSet<RFQItem> RFQItems { get; set; } = null!;
        public DbSet<SupplierOffer> SupplierOffers { get; set; } = null!;

        #endregion

        #region Workflow Entities

        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<ProjectStage> ProjectStages { get; set; } = null!;
        public DbSet<ProjectActivity> ProjectActivities { get; set; } = null!;
        public DbSet<ProjectDocument> ProjectDocuments { get; set; } = null!;
        public DbSet<ProjectMessage> ProjectMessages { get; set; } = null!;
        public DbSet<ProjectNotification> ProjectNotifications { get; set; } = null!;
        public DbSet<ProjectTeamMember> ProjectTeamMembers { get; set; } = null!;
        public DbSet<WorkflowTemplate> WorkflowTemplates { get; set; } = null!;
        public DbSet<WorkflowStage> WorkflowStages { get; set; } = null!;

        #endregion

        protected override void ApplyEntityConfigurations(ModelBuilder modelBuilder)
        {
            // Apply configurations with schema organization

            // Core schema entities
            ConfigureCoreEntities(modelBuilder);

            // Trading schema entities
            ConfigureTradingEntities(modelBuilder);

            // Workflow entities
            ConfigureWorkflowEntities(modelBuilder);
        }

        private void ConfigureCoreEntities(ModelBuilder modelBuilder)
        {
            // Company configuration
            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("Companies", "dbo"); // Will move to Core schema later
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.CompanyType);
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(256);
                entity.Property(e => e.Website).HasMaxLength(500);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users", "dbo");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.AspNetUserId).IsUnique();
                entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products", "dbo"); // Will move to Core schema later
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SKU).IsUnique();
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.IsActive);
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.SKU).HasMaxLength(100);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.Description).HasMaxLength(2000);

                // Relationships
                entity.HasOne(e => e.Supplier)
                    .WithMany(s => s.Products)
                    .HasForeignKey(e => e.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Buyer configuration
            modelBuilder.Entity<Buyer>(entity =>
            {
                entity.ToTable("FoodXBuyers", "dbo");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Company).HasMaxLength(500);
                entity.Property(e => e.Type).HasMaxLength(200);
                entity.Property(e => e.Region).HasMaxLength(200);
            });

            // Supplier configuration
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.ToTable("FoodXSuppliers", "dbo");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SupplierName).HasMaxLength(500);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.ProductCategory).HasMaxLength(500);

                // Navigation property for products
                entity.HasMany(s => s.Products)
                    .WithOne(p => p.Supplier)
                    .HasForeignKey(p => p.SupplierId);
            });

            // Exhibition configuration
            modelBuilder.Entity<Exhibition>(entity =>
            {
                entity.ToTable("Exhibitions", "dbo");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.City).HasMaxLength(200);
                entity.Property(e => e.Venue).HasMaxLength(500);
                entity.Property(e => e.Description).HasMaxLength(2000);

                entity.HasMany(e => e.Exhibitors)
                    .WithOne(ex => ex.Exhibition)
                    .HasForeignKey(ex => ex.ExhibitionId);
            });

            // Exhibitor configuration
            modelBuilder.Entity<Exhibitor>(entity =>
            {
                entity.ToTable("Exhibitors", "dbo");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CompanyName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Contact).HasMaxLength(500);
                entity.Property(e => e.Email).HasMaxLength(256);
                entity.Property(e => e.Phone).HasMaxLength(50);

                entity.HasOne(e => e.Exhibition)
                    .WithMany(ex => ex.Exhibitors)
                    .HasForeignKey(e => e.ExhibitionId);
            });
        }

        private void ConfigureTradingEntities(ModelBuilder modelBuilder)
        {
            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders", "dbo"); // Will move to Trading schema later
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.OrderDate);
                entity.Property(e => e.OrderNumber).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);

                // Relationships
                entity.HasOne(o => o.Buyer)
                    .WithMany()
                    .HasForeignKey(o => o.BuyerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Supplier)
                    .WithMany()
                    .HasForeignKey(o => o.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(o => o.OrderItems)
                    .WithOne(oi => oi.Order)
                    .HasForeignKey(oi => oi.OrderId);
            });

            // OrderItem configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems", "dbo");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).HasPrecision(18, 2);
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);

                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId);

                entity.HasOne(oi => oi.Product)
                    .WithMany()
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Quote configuration
            modelBuilder.Entity<Quote>(entity =>
            {
                entity.ToTable("Quotes", "dbo");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.QuoteNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.Property(e => e.QuoteNumber).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);

                entity.HasMany(q => q.Items)
                    .WithOne(qi => qi.Quote)
                    .HasForeignKey(qi => qi.QuoteId);
            });

            // QuoteItem configuration
            modelBuilder.Entity<QuoteItem>(entity =>
            {
                entity.ToTable("QuoteItems", "dbo");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).HasPrecision(18, 2);
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            });

            // RFQ configuration
            modelBuilder.Entity<RFQ>(entity =>
            {
                entity.ToTable("RFQs", "dbo");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.RFQNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.Property(e => e.RFQNumber).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.Status).HasMaxLength(50);

                entity.HasMany(r => r.Items)
                    .WithOne(ri => ri.RFQ)
                    .HasForeignKey(ri => ri.RFQId);
            });

            // RFQItem configuration
            modelBuilder.Entity<RFQItem>(entity =>
            {
                entity.ToTable("RFQItems", "dbo");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductName).HasMaxLength(200);
                entity.Property(e => e.Specifications).HasMaxLength(2000);
                entity.Property(e => e.Quantity).HasPrecision(18, 2);
                entity.Property(e => e.Unit).HasMaxLength(50);
            });
        }

        private void ConfigureWorkflowEntities(ModelBuilder modelBuilder)
        {
            // Project configuration
            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("Projects", "dbo");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProjectNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.Property(e => e.ProjectNumber).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ProjectName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.Status).HasMaxLength(50);

                entity.HasMany(p => p.Stages)
                    .WithOne(ps => ps.Project)
                    .HasForeignKey(ps => ps.ProjectId);

                entity.HasMany(p => p.Activities)
                    .WithOne(pa => pa.Project)
                    .HasForeignKey(pa => pa.ProjectId);

                entity.HasMany(p => p.TeamMembers)
                    .WithOne(tm => tm.Project)
                    .HasForeignKey(tm => tm.ProjectId);
            });

            // ProjectStage configuration
            modelBuilder.Entity<ProjectStage>(entity =>
            {
                entity.ToTable("ProjectStages", "dbo");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StageName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.Status).HasMaxLength(50);
            });

            // WorkflowTemplate configuration
            modelBuilder.Entity<WorkflowTemplate>(entity =>
            {
                entity.ToTable("WorkflowTemplates", "dbo");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TemplateName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.TemplateType).HasMaxLength(100);

                entity.HasMany(wt => wt.Stages)
                    .WithOne(ws => ws.Template)
                    .HasForeignKey(ws => ws.TemplateId);
            });
        }
    }
}
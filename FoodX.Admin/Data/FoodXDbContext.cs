using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FoodX.Admin.Models;

namespace FoodX.Admin.Data
{
    public class FoodXDbContext : IdentityDbContext<ApplicationUser>
    {
        public FoodXDbContext(DbContextOptions<FoodXDbContext> options)
            : base(options)
        {
        }

        // Custom tables (existing in database)
        public DbSet<FoodXUser> FoodXUsers { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<UserEmployment> UserEmployments { get; set; }
        public DbSet<UserPhone> UserPhones { get; set; }
        public DbSet<Product> Products { get; set; }

        // Role-specific tables
        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Expert> Experts { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<SystemAdmin> SystemAdmins { get; set; }
        public DbSet<BackOffice> BackOffices { get; set; }

        // Invitation system
        public DbSet<Invitation> Invitations { get; set; }

        // External data tables (imported from external sources)
        public DbSet<FoodXBuyer> FoodXBuyers { get; set; }

        // AI Request System tables
        public DbSet<BuyerRequest> BuyerRequests { get; set; }
        public DbSet<AIAnalysisResult> AIAnalysisResults { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            base.OnModelCreating(builder);

            // Configure FoodXUser
            builder.Entity<FoodXUser>(entity =>
            {
                entity.ToTable("Users", t =>
                {
                    t.HasCheckConstraint("CK_Users_Role",
                        "[Role] IN ('Buyer', 'Supplier', 'Agent', 'Admin', 'Expert', 'SuperAdmin')");
                });
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

                // Computed column for FullName
                entity.Ignore(e => e.FullName);
            });

            // Configure Company
            builder.Entity<Company>(entity =>
            {
                entity.ToTable("Companies", t =>
                {
                    t.HasCheckConstraint("CK_Companies_CompanyType",
                        "[CompanyType] IN ('Buyer', 'Supplier')");
                });
                entity.Property(e => e.CompanyType).HasDefaultValue("Buyer");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            });

            // Configure UserEmployment
            builder.Entity<UserEmployment>(entity =>
            {
                entity.ToTable("UserEmployments");
                entity.Property(e => e.IsPrimary).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

                // Unique constraint on UserId + CompanyId
                entity.HasIndex(e => new { e.UserId, e.CompanyId }).IsUnique();

                // Relationships
                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserEmployments)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Company)
                    .WithMany(c => c.UserEmployments)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure UserPhone
            builder.Entity<UserPhone>(entity =>
            {
                entity.ToTable("UserPhones");

                // Relationship
                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserPhones)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure role-specific tables
            builder.Entity<Buyer>(entity =>
            {
                entity.ToTable("Buyers");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
                entity.HasOne(e => e.Company).WithMany().HasForeignKey(e => e.CompanyId);
            });

            builder.Entity<Supplier>(entity =>
            {
                entity.ToTable("Suppliers");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
                entity.HasOne(e => e.Company).WithMany().HasForeignKey(e => e.CompanyId);
            });

            builder.Entity<Expert>(entity =>
            {
                entity.ToTable("Experts");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
            });

            builder.Entity<Agent>(entity =>
            {
                entity.ToTable("Agents");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
            });

            builder.Entity<SystemAdmin>(entity =>
            {
                entity.ToTable("SystemAdmins");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.AccessLevel).HasDefaultValue("Full");
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
            });

            builder.Entity<BackOffice>(entity =>
            {
                entity.ToTable("BackOffice");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
            });

            // Configure Product
            builder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(e => e.IsOrganic).HasDefaultValue(false);
                entity.Property(e => e.IsAvailable).HasDefaultValue(true);
                entity.Property(e => e.MinOrderQuantity).HasDefaultValue(1);
                entity.Property(e => e.StockQuantity).HasDefaultValue(0);

                // Indexes
                entity.HasIndex(e => e.Category).HasDatabaseName("IX_Products_Category");
                entity.HasIndex(e => e.SupplierId).HasDatabaseName("IX_Products_SupplierId");
                entity.HasIndex(e => e.CompanyId).HasDatabaseName("IX_Products_CompanyId");

                // Relationships - explicitly configure to avoid shadow properties
                entity.HasOne(e => e.Supplier)
                    .WithMany(s => s.Products)
                    .HasForeignKey(e => e.SupplierId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);

                entity.HasOne(e => e.Company)
                    .WithMany(c => c.Products)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
            });

            // Configure FoodXBuyer (external data table - read-only)
            builder.Entity<FoodXBuyer>(entity =>
            {
                entity.ToTable("FoodXBuyers");
                entity.HasKey(e => e.Id);

                // Indexes for common queries
                entity.HasIndex(e => e.Company).HasDatabaseName("IX_FoodXBuyers_Company");
                entity.HasIndex(e => e.Region).HasDatabaseName("IX_FoodXBuyers_Region");
                entity.HasIndex(e => e.Type).HasDatabaseName("IX_FoodXBuyers_Type");
            });

            // Configure BuyerRequest
            builder.Entity<BuyerRequest>(entity =>
            {
                entity.ToTable("BuyerRequests");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.InputType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.InputContent).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Indexes
                entity.HasIndex(e => e.BuyerId).HasDatabaseName("IX_BuyerRequests_BuyerId");
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_BuyerRequests_Status");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_BuyerRequests_CreatedAt");

                // Relationship with FoodXBuyer
                entity.HasOne(e => e.Buyer)
                    .WithMany()
                    .HasForeignKey(e => e.BuyerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure AIAnalysisResult
            builder.Entity<AIAnalysisResult>(entity =>
            {
                entity.ToTable("AIAnalysisResults");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AnalysisData).IsRequired();
                entity.Property(e => e.ConfidenceScore).HasColumnType("decimal(5,2)");
                entity.Property(e => e.AIProvider).HasMaxLength(50);
                entity.Property(e => e.ProcessedAt).HasDefaultValueSql("GETUTCDATE()");

                // Index
                entity.HasIndex(e => e.RequestId).HasDatabaseName("IX_AIAnalysisResults_RequestId");

                // Relationship with BuyerRequest
                entity.HasOne(e => e.Request)
                    .WithMany(r => r.AnalysisResults)
                    .HasForeignKey(e => e.RequestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
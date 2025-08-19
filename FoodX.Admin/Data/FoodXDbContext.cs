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
                        "[Role] IN ('Buyer', 'Seller', 'Agent', 'Admin')");
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
        }
    }
}
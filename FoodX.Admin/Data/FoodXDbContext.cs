using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FoodX.Admin.Models;
using FoodX.Admin.Data.Configurations;

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
        public DbSet<FoodXSupplier> FoodXSuppliers { get; set; }

        // AI Request System tables
        public DbSet<BuyerRequest> BuyerRequests { get; set; }
        public DbSet<AIAnalysisResult> AIAnalysisResults { get; set; }

        // Supplier Products table
        public DbSet<SupplierProduct> SupplierProducts { get; set; }

        // Import History table
        public DbSet<ImportHistory> ImportHistories { get; set; }
        
        // AI Request Brief table
        public DbSet<AIRequestBrief> AIRequestBriefs { get; set; }

        // Project Management tables
        public DbSet<Project> Projects { get; set; }

        // Billing System tables
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<ShipmentItem> ShipmentItems { get; set; }

        // Notification System tables
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<DirectMessage> DirectMessages { get; set; }

        // Additional tables
        public DbSet<ShipmentInspection> ShipmentInspections { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceLineItem> InvoiceLineItems { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<CommissionRate> CommissionRates { get; set; }
        public DbSet<CommissionTier> CommissionTiers { get; set; }
        public DbSet<CustomCommissionAgreement> CustomCommissionAgreements { get; set; }
        public DbSet<BillingProfile> BillingProfiles { get; set; }
        public DbSet<TaxConfiguration> TaxConfigurations { get; set; }
        
        // Recurring Order System tables
        public DbSet<RecurringOrderTemplate> RecurringOrderTemplates { get; set; }
        public DbSet<RecurringOrderItem> RecurringOrderItems { get; set; }
        public DbSet<RecurringOrderHistory> RecurringOrderHistories { get; set; }

        // RFQ System tables
        public DbSet<RFQ> RFQs { get; set; }
        public DbSet<RFQSupplierMatch> RFQSupplierMatches { get; set; }
        public DbSet<RFQAttachment> RFQAttachments { get; set; }
        public DbSet<SupplierQuote> SupplierQuotes { get; set; }
        public DbSet<QuotePriceTier> QuotePriceTiers { get; set; }
        public DbSet<SupplierProductCatalog> SupplierProductCatalog { get; set; }

        // Buyer-Specific Pricing & Negotiation tables
        public DbSet<BuyerSpecificPricing> BuyerSpecificPricings { get; set; }
        public DbSet<BuyerPriceTier> BuyerPriceTiers { get; set; }
        public DbSet<NegotiationHistory> NegotiationHistories { get; set; }
        public DbSet<NegotiationRound> NegotiationRounds { get; set; }
        public DbSet<NegotiationEmailTemplate> NegotiationEmailTemplates { get; set; }

        // Compliance Verification tables
        public DbSet<ComplianceVerification> ComplianceVerifications { get; set; }
        public DbSet<CertificationDocument> CertificationDocuments { get; set; }
        public DbSet<ComplianceChecklistItem> ComplianceChecklistItems { get; set; }
        public DbSet<LabTestResult> LabTestResults { get; set; }

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

            // Configure FoodXSupplier - Fix decimal precision warnings and add performance indexes
            builder.Entity<FoodXSupplier>(entity =>
            {
                entity.ToTable("FoodXSuppliers");
                entity.Property(e => e.ExportPercentage).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Rating).HasColumnType("decimal(3,2)");
                
                // Performance indexes for frequently queried columns
                entity.HasIndex(e => e.Country).HasDatabaseName("IX_FoodXSuppliers_Country");
                entity.HasIndex(e => e.ProductCategory).HasDatabaseName("IX_FoodXSuppliers_ProductCategory");
                entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_FoodXSuppliers_IsActive");
                entity.HasIndex(e => e.IsVerified).HasDatabaseName("IX_FoodXSuppliers_IsVerified");
                entity.HasIndex(e => e.Rating).HasDatabaseName("IX_FoodXSuppliers_Rating");
                entity.HasIndex(e => new { e.Country, e.IsActive }).HasDatabaseName("IX_FoodXSuppliers_Country_IsActive");
                entity.HasIndex(e => new { e.ProductCategory, e.IsActive }).HasDatabaseName("IX_FoodXSuppliers_Category_IsActive");
                
                // Certification indexes for filtering
                entity.HasIndex(e => e.IsKosherCertified).HasDatabaseName("IX_FoodXSuppliers_Kosher");
                entity.HasIndex(e => e.IsHalalCertified).HasDatabaseName("IX_FoodXSuppliers_Halal");
                entity.HasIndex(e => e.IsOrganicCertified).HasDatabaseName("IX_FoodXSuppliers_Organic");
                entity.HasIndex(e => e.IsGlutenFreeCertified).HasDatabaseName("IX_FoodXSuppliers_GlutenFree");
            });

            // Configure SupplierProduct - Fix decimal precision warnings and add indexes
            builder.Entity<SupplierProduct>(entity =>
            {
                entity.ToTable("SupplierProducts");
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                
                // Performance indexes
                entity.HasIndex(e => e.SupplierId).HasDatabaseName("IX_SupplierProducts_SupplierId");
                entity.HasIndex(e => e.Category).HasDatabaseName("IX_SupplierProducts_Category");
                entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_SupplierProducts_IsActive");
                entity.HasIndex(e => new { e.SupplierId, e.IsActive }).HasDatabaseName("IX_SupplierProducts_Supplier_Active");
                entity.HasIndex(e => e.ProductName).HasDatabaseName("IX_SupplierProducts_ProductName");
            });
            
            // Configure Product with additional indexes
            builder.Entity<Product>(entity =>
            {
                entity.HasIndex(e => e.Category).HasDatabaseName("IX_Products_Category");
                entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_Products_IsActive");
                entity.HasIndex(e => e.IsAvailable).HasDatabaseName("IX_Products_IsAvailable");
                entity.HasIndex(e => e.SupplierId).HasDatabaseName("IX_Products_SupplierId");
                entity.HasIndex(e => e.CompanyId).HasDatabaseName("IX_Products_CompanyId");
                entity.HasIndex(e => new { e.Category, e.IsActive, e.IsAvailable })
                    .HasDatabaseName("IX_Products_Category_Active_Available");
                entity.HasIndex(e => e.IsOrganic).HasDatabaseName("IX_Products_IsOrganic");
            });
            
            // Configure FoodXBuyer with indexes
            builder.Entity<FoodXBuyer>(entity =>
            {
                entity.HasIndex(e => e.Company).HasDatabaseName("IX_FoodXBuyers_Company");
                entity.HasIndex(e => e.Type).HasDatabaseName("IX_FoodXBuyers_Type");
                entity.HasIndex(e => e.Region).HasDatabaseName("IX_FoodXBuyers_Region");
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_FoodXBuyers_Status");
            });

            // Configure Order
            builder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CommissionAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasIndex(e => e.OrderNumber).IsUnique().HasDatabaseName("IX_Orders_OrderNumber");
                entity.HasIndex(e => e.BuyerId).HasDatabaseName("IX_Orders_BuyerId");
                entity.HasIndex(e => e.SupplierId).HasDatabaseName("IX_Orders_SupplierId");
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_Orders_Status");
                entity.HasIndex(e => e.OrderDate).HasDatabaseName("IX_Orders_OrderDate");
                
                entity.HasOne(e => e.Buyer)
                    .WithMany()
                    .HasForeignKey(e => e.BuyerId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Supplier)
                    .WithMany()
                    .HasForeignKey(e => e.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure OrderItem
            builder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");
                entity.Property(e => e.Quantity).HasColumnType("decimal(18,2)");
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasIndex(e => e.OrderId).HasDatabaseName("IX_OrderItems_OrderId");
                entity.HasIndex(e => e.ProductId).HasDatabaseName("IX_OrderItems_ProductId");
                
                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Shipment
            builder.Entity<Shipment>(entity =>
            {
                entity.ToTable("Shipments");
                entity.Property(e => e.ShipmentValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.InsuranceValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasIndex(e => e.ShipmentNumber).IsUnique().HasDatabaseName("IX_Shipments_ShipmentNumber");
                entity.HasIndex(e => e.OrderId).HasDatabaseName("IX_Shipments_OrderId");
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_Shipments_Status");
                entity.HasIndex(e => e.TrackingNumber).HasDatabaseName("IX_Shipments_TrackingNumber");
                
                entity.HasOne(e => e.Order)
                    .WithMany(o => o.Shipments)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ShipmentItem
            builder.Entity<ShipmentItem>(entity =>
            {
                entity.ToTable("ShipmentItems");
                entity.Property(e => e.QuantityShipped).HasColumnType("decimal(18,2)");
                entity.Property(e => e.QuantityReceived).HasColumnType("decimal(18,2)");
                entity.Property(e => e.QuantityAccepted).HasColumnType("decimal(18,2)");
                entity.Property(e => e.QuantityRejected).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasIndex(e => e.ShipmentId).HasDatabaseName("IX_ShipmentItems_ShipmentId");
                entity.HasIndex(e => e.OrderItemId).HasDatabaseName("IX_ShipmentItems_OrderItemId");
                
                entity.HasOne(e => e.Shipment)
                    .WithMany(s => s.ShipmentItems)
                    .HasForeignKey(e => e.ShipmentId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.OrderItem)
                    .WithMany()
                    .HasForeignKey(e => e.OrderItemId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ShipmentInspection
            builder.Entity<ShipmentInspection>(entity =>
            {
                entity.ToTable("ShipmentInspections");
                entity.Property(e => e.AcceptanceRate).HasColumnType("decimal(5,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasIndex(e => e.ShipmentId).HasDatabaseName("IX_ShipmentInspections_ShipmentId");
                entity.HasIndex(e => e.InspectionDate).HasDatabaseName("IX_ShipmentInspections_InspectionDate");
                
                entity.HasOne(e => e.Shipment)
                    .WithMany(s => s.Inspections)
                    .HasForeignKey(e => e.ShipmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Invoice
            builder.Entity<Invoice>(entity =>
            {
                entity.ToTable("Invoices");
                entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxRate).HasColumnType("decimal(5,2)");
                entity.Property(e => e.ShippingAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaidAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.BalanceDue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasIndex(e => e.InvoiceNumber).IsUnique().HasDatabaseName("IX_Invoices_InvoiceNumber");
                entity.HasIndex(e => e.OrderId).HasDatabaseName("IX_Invoices_OrderId");
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_Invoices_Status");
                entity.HasIndex(e => e.InvoiceDate).HasDatabaseName("IX_Invoices_InvoiceDate");
                entity.HasIndex(e => e.DueDate).HasDatabaseName("IX_Invoices_DueDate");
                
                entity.HasOne(e => e.Order)
                    .WithMany(o => o.Invoices)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Shipment)
                    .WithMany()
                    .HasForeignKey(e => e.ShipmentId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.Buyer)
                    .WithMany()
                    .HasForeignKey(e => e.BuyerId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.Supplier)
                    .WithMany()
                    .HasForeignKey(e => e.SupplierId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure InvoiceLineItem
            builder.Entity<InvoiceLineItem>(entity =>
            {
                entity.ToTable("InvoiceLineItems");
                entity.Property(e => e.Quantity).HasColumnType("decimal(18,2)");
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxRate).HasColumnType("decimal(5,2)");
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasIndex(e => e.InvoiceId).HasDatabaseName("IX_InvoiceLineItems_InvoiceId");
                
                entity.HasOne(e => e.Invoice)
                    .WithMany(i => i.LineItems)
                    .HasForeignKey(e => e.InvoiceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure PaymentTransaction
            builder.Entity<PaymentTransaction>(entity =>
            {
                entity.ToTable("PaymentTransactions");
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ProcessingFee).HasColumnType("decimal(18,2)");
                entity.Property(e => e.NetAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasIndex(e => e.TransactionNumber).IsUnique().HasDatabaseName("IX_PaymentTransactions_TransactionNumber");
                entity.HasIndex(e => e.InvoiceId).HasDatabaseName("IX_PaymentTransactions_InvoiceId");
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_PaymentTransactions_Status");
                entity.HasIndex(e => e.TransactionDate).HasDatabaseName("IX_PaymentTransactions_TransactionDate");
                
                entity.HasOne(e => e.Invoice)
                    .WithMany(i => i.Payments)
                    .HasForeignKey(e => e.InvoiceId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure CommissionRate
            builder.Entity<CommissionRate>(entity =>
            {
                entity.ToTable("CommissionRates");
                entity.Property(e => e.BaseRatePercent).HasColumnType("decimal(5,2)");
                entity.Property(e => e.MinimumFee).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MaximumFee).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasIndex(e => e.RateType).HasDatabaseName("IX_CommissionRates_RateType");
                entity.HasIndex(e => e.ProductCategory).HasDatabaseName("IX_CommissionRates_ProductCategory");
                entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_CommissionRates_IsActive");
            });

            // Configure CommissionTier
            builder.Entity<CommissionTier>(entity =>
            {
                entity.ToTable("CommissionTiers");
                entity.Property(e => e.MinVolume).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MaxVolume).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RatePercent).HasColumnType("decimal(5,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasIndex(e => e.CommissionRateId).HasDatabaseName("IX_CommissionTiers_CommissionRateId");
                
                entity.HasOne(e => e.CommissionRate)
                    .WithMany(r => r.Tiers)
                    .HasForeignKey(e => e.CommissionRateId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CustomCommissionAgreement
            builder.Entity<CustomCommissionAgreement>(entity =>
            {
                entity.ToTable("CustomCommissionAgreements");
                entity.Property(e => e.CustomRatePercent).HasColumnType("decimal(5,2)");
                entity.Property(e => e.FixedFee).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MinimumMonthlyVolume).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MaximumMonthlyVolume).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasIndex(e => e.AgreementNumber).IsUnique().HasDatabaseName("IX_CustomCommissionAgreements_AgreementNumber");
                entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_CustomCommissionAgreements_IsActive");
                
                entity.HasOne(e => e.Buyer)
                    .WithMany()
                    .HasForeignKey(e => e.BuyerId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.Supplier)
                    .WithMany()
                    .HasForeignKey(e => e.SupplierId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.CommissionRate)
                    .WithMany(r => r.CustomAgreements)
                    .HasForeignKey(e => e.CommissionRateId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure BillingProfile
            builder.Entity<BillingProfile>(entity =>
            {
                entity.ToTable("BillingProfiles");
                entity.Property(e => e.CreditLimit).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CurrentBalance).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasIndex(e => e.BuyerId).HasDatabaseName("IX_BillingProfiles_BuyerId");
                entity.HasIndex(e => e.SupplierId).HasDatabaseName("IX_BillingProfiles_SupplierId");
                
                entity.HasOne(e => e.Buyer)
                    .WithMany()
                    .HasForeignKey(e => e.BuyerId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.Supplier)
                    .WithMany()
                    .HasForeignKey(e => e.SupplierId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure TaxConfiguration
            builder.Entity<TaxConfiguration>(entity =>
            {
                entity.ToTable("TaxConfigurations");
                entity.Property(e => e.StandardRate).HasColumnType("decimal(5,2)");
                entity.Property(e => e.ReducedRate).HasColumnType("decimal(5,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasIndex(e => e.Country).HasDatabaseName("IX_TaxConfigurations_Country");
                entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_TaxConfigurations_IsActive");
            });

            // Configure RecurringOrderItem
            builder.Entity<RecurringOrderItem>(entity =>
            {
                entity.ToTable("RecurringOrderItems");
                entity.Property(e => e.Quantity).HasColumnType("decimal(18,2)");
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasIndex(e => e.RecurringOrderTemplateId).HasDatabaseName("IX_RecurringOrderItems_TemplateId");
                entity.HasIndex(e => e.ProductId).HasDatabaseName("IX_RecurringOrderItems_ProductId");
                
                entity.HasOne(e => e.RecurringOrderTemplate)
                    .WithMany(t => t.Items)
                    .HasForeignKey(e => e.RecurringOrderTemplateId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure RFQ
            builder.Entity<RFQ>(entity =>
            {
                entity.ToTable("RFQs");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.TargetPriceMin).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TargetPriceMax).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TargetQuantity).HasColumnType("decimal(18,2)");
                
                entity.HasIndex(e => e.RFQNumber).IsUnique().HasDatabaseName("IX_RFQs_RFQNumber");
                entity.HasIndex(e => e.BuyerId).HasDatabaseName("IX_RFQs_BuyerId");
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_RFQs_Status");
                entity.HasIndex(e => e.QuoteDeadline).HasDatabaseName("IX_RFQs_QuoteDeadline");
                entity.HasIndex(e => e.ProductCategory).HasDatabaseName("IX_RFQs_ProductCategory");
                
                entity.HasOne(e => e.Buyer)
                    .WithMany()
                    .HasForeignKey(e => e.BuyerId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.BuyerRequest)
                    .WithMany()
                    .HasForeignKey(e => e.BuyerRequestId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.AwardedSupplier)
                    .WithMany()
                    .HasForeignKey(e => e.AwardedSupplierId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Project - Fix decimal precision warnings
            builder.Entity<Project>(entity =>
            {
                entity.ToTable("Projects");
                entity.Property(e => e.ActualCost).HasColumnType("decimal(18,2)");
                entity.Property(e => e.EstimatedBudget).HasColumnType("decimal(18,2)");
            });

            // Configure RFQSupplierMatch
            builder.Entity<RFQSupplierMatch>(entity =>
            {
                entity.ToTable("RFQSupplierMatches");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.MatchScore).HasColumnType("decimal(5,2)");
                entity.Property(e => e.QuotedPrice).HasColumnType("decimal(18,2)");
                
                entity.HasIndex(e => new { e.RFQId, e.SupplierId }).IsUnique().HasDatabaseName("IX_RFQSupplierMatches_RFQ_Supplier");
                entity.HasIndex(e => e.InvitationStatus).HasDatabaseName("IX_RFQSupplierMatches_Status");
                entity.HasIndex(e => e.MatchScore).HasDatabaseName("IX_RFQSupplierMatches_Score");
                
                entity.HasOne(e => e.RFQ)
                    .WithMany(r => r.SupplierMatches)
                    .HasForeignKey(e => e.RFQId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Supplier)
                    .WithMany()
                    .HasForeignKey(e => e.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Quote)
                    .WithMany()
                    .HasForeignKey(e => e.QuoteId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure SupplierQuote
            builder.Entity<SupplierQuote>(entity =>
            {
                entity.ToTable("SupplierQuotes");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CartonPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.AvailableQuantity).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MinimumOrderQuantity).HasColumnType("decimal(18,2)");
                entity.Property(e => e.GrossWeight).HasColumnType("decimal(18,3)");
                entity.Property(e => e.NetWeight).HasColumnType("decimal(18,3)");
                entity.Property(e => e.UnitsPerCarton).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MinTemperature).HasColumnType("decimal(5,2)");
                entity.Property(e => e.MaxTemperature).HasColumnType("decimal(5,2)");
                
                entity.HasIndex(e => e.QuoteNumber).IsUnique().HasDatabaseName("IX_SupplierQuotes_QuoteNumber");
                entity.HasIndex(e => e.RFQId).HasDatabaseName("IX_SupplierQuotes_RFQId");
                entity.HasIndex(e => e.SupplierId).HasDatabaseName("IX_SupplierQuotes_SupplierId");
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_SupplierQuotes_Status");
                entity.HasIndex(e => e.ValidUntil).HasDatabaseName("IX_SupplierQuotes_ValidUntil");
                
                entity.HasOne(e => e.RFQ)
                    .WithMany(r => r.Quotes)
                    .HasForeignKey(e => e.RFQId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Supplier)
                    .WithMany()
                    .HasForeignKey(e => e.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure QuotePriceTier
            builder.Entity<QuotePriceTier>(entity =>
            {
                entity.ToTable("QuotePriceTiers");
                entity.Property(e => e.MinQuantity).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MaxQuantity).HasColumnType("decimal(18,2)");
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,4)");
                entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5,2)");
                
                entity.HasIndex(e => e.QuoteId).HasDatabaseName("IX_QuotePriceTiers_QuoteId");
                
                entity.HasOne(e => e.Quote)
                    .WithMany(q => q.PriceTiers)
                    .HasForeignKey(e => e.QuoteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure RFQAttachment
            builder.Entity<RFQAttachment>(entity =>
            {
                entity.ToTable("RFQAttachments");
                entity.Property(e => e.UploadedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasIndex(e => e.RFQId).HasDatabaseName("IX_RFQAttachments_RFQId");
                
                entity.HasOne(e => e.RFQ)
                    .WithMany(r => r.Attachments)
                    .HasForeignKey(e => e.RFQId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure SupplierProductCatalog
            builder.Entity<SupplierProductCatalog>(entity =>
            {
                entity.ToTable("SupplierProductCatalog");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UnitWholesalePrice).HasColumnType("decimal(18,4)");
                entity.Property(e => e.InitialUnitPrice).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CartonWholesalePrice).HasColumnType("decimal(18,4)");
                entity.Property(e => e.GrossWeight).HasColumnType("decimal(18,3)");
                entity.Property(e => e.NetWeight).HasColumnType("decimal(18,3)");
                entity.Property(e => e.MinTemperature).HasColumnType("decimal(5,2)");
                entity.Property(e => e.MaxTemperature).HasColumnType("decimal(5,2)");
                
                entity.HasIndex(e => e.SupplierId).HasDatabaseName("IX_SupplierProductCatalog_SupplierId");
                entity.HasIndex(e => e.ProductName).HasDatabaseName("IX_SupplierProductCatalog_ProductName");
                entity.HasIndex(e => e.Category).HasDatabaseName("IX_SupplierProductCatalog_Category");
                entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_SupplierProductCatalog_IsActive");
                entity.HasIndex(e => e.IsKosher).HasDatabaseName("IX_SupplierProductCatalog_IsKosher");
                entity.HasIndex(e => e.IsHalal).HasDatabaseName("IX_SupplierProductCatalog_IsHalal");
                entity.HasIndex(e => e.IsOrganic).HasDatabaseName("IX_SupplierProductCatalog_IsOrganic");
                entity.HasIndex(e => new { e.SupplierId, e.IsActive }).HasDatabaseName("IX_SupplierProductCatalog_Supplier_Active");
                
                entity.HasOne(e => e.Supplier)
                    .WithMany()
                    .HasForeignKey(e => e.SupplierId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Apply FoodXSupplier configuration to handle type conversions
            builder.ApplyConfiguration(new FoodXSupplierConfiguration());
            builder.ApplyConfiguration(new ProjectConfiguration());
        }
    }
}
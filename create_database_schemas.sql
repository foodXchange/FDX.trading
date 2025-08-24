-- ============================================
-- Create Database Schemas for FDX.Trading
-- ============================================
-- This script creates logical schemas to organize tables
-- while keeping them in the same database
-- ============================================

-- Create Core Schema (Shared entities)
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Core')
BEGIN
    EXEC('CREATE SCHEMA Core');
    PRINT 'Created schema: Core';
END
GO

-- Create Trading Schema (Transactional entities)
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Trading')
BEGIN
    EXEC('CREATE SCHEMA Trading');
    PRINT 'Created schema: Trading';
END
GO

-- Create Analytics Schema (Reporting and analytics)
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Analytics')
BEGIN
    EXEC('CREATE SCHEMA Analytics');
    PRINT 'Created schema: Analytics';
END
GO

-- Create Portal Schema (Portal-specific data)
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Portal')
BEGIN
    EXEC('CREATE SCHEMA Portal');
    PRINT 'Created schema: Portal';
END
GO

-- ============================================
-- Move existing tables to appropriate schemas
-- ============================================

-- Core Schema Tables (shared across all portals)
-- Note: We'll move these gradually to avoid breaking existing code

-- Companies -> Core.Companies
-- Products -> Core.Products
-- Users -> Core.Users
-- FoodXBuyers -> Core.Buyers
-- FoodXSuppliers -> Core.Suppliers

-- Trading Schema Tables
-- Orders -> Trading.Orders
-- OrderItems -> Trading.OrderItems
-- Quotes -> Trading.Quotes
-- QuoteItems -> Trading.QuoteItems
-- RFQ -> Trading.RFQ
-- RFQItems -> Trading.RFQItems

-- Analytics Schema Tables
-- VectorStore -> Analytics.VectorStore
-- VectorDimensions -> Analytics.VectorDimensions
-- SearchHistory -> Analytics.SearchHistory

-- Portal Schema Tables
-- RolePortals -> Portal.RolePortals
-- UserPreferences -> Portal.UserPreferences

-- ============================================
-- Create synonyms for backward compatibility
-- This allows existing code to work while we migrate
-- ============================================

-- Example: When we move Products to Core schema
-- IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL
-- BEGIN
--     ALTER SCHEMA Core TRANSFER dbo.Products;
--     CREATE SYNONYM dbo.Products FOR Core.Products;
-- END

-- ============================================
-- Grant permissions to schemas
-- ============================================

-- Grant permissions to application user
DECLARE @AppUser NVARCHAR(128) = 'foodxapp';

-- Core schema permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::Core TO [foodxapp];
GRANT EXECUTE ON SCHEMA::Core TO [foodxapp];

-- Trading schema permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::Trading TO [foodxapp];
GRANT EXECUTE ON SCHEMA::Trading TO [foodxapp];

-- Analytics schema permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::Analytics TO [foodxapp];
GRANT EXECUTE ON SCHEMA::Analytics TO [foodxapp];

-- Portal schema permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::Portal TO [foodxapp];
GRANT EXECUTE ON SCHEMA::Portal TO [foodxapp];

PRINT 'Database schemas created and permissions granted successfully!';
GO

-- ============================================
-- Create helper views for each portal
-- ============================================

-- Buyer Portal View
CREATE OR ALTER VIEW Portal.BuyerDashboardData
AS
SELECT 
    c.Id AS CompanyId,
    c.Name AS CompanyName,
    COUNT(DISTINCT p.Id) AS TotalProducts,
    COUNT(DISTINCT o.Id) AS TotalOrders
FROM Companies c
LEFT JOIN Products p ON p.IsActive = 1
LEFT JOIN Orders o ON o.BuyerId = c.Id
GROUP BY c.Id, c.Name;
GO

-- Supplier Portal View
CREATE OR ALTER VIEW Portal.SupplierDashboardData
AS
SELECT 
    c.Id AS CompanyId,
    c.Name AS CompanyName,
    COUNT(DISTINCT p.Id) AS MyProducts,
    COUNT(DISTINCT o.Id) AS ReceivedOrders
FROM Companies c
LEFT JOIN Products p ON p.SupplierId = c.Id AND p.IsActive = 1
LEFT JOIN Orders o ON o.SupplierId = c.Id
GROUP BY c.Id, c.Name;
GO

PRINT 'Portal views created successfully!';
GO
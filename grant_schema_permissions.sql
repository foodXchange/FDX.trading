-- ============================================
-- Grant Schema Permissions
-- ============================================

-- Check if we're using Azure AD authentication
PRINT 'Current user: ' + SUSER_NAME();
PRINT 'Current database: ' + DB_NAME();

-- Create views for each portal (these will work regardless of user)
-- ============================================

-- Buyer Portal View
CREATE OR ALTER VIEW Portal.BuyerDashboardData
AS
SELECT 
    c.Id AS CompanyId,
    c.Name AS CompanyName,
    COUNT(DISTINCT p.Id) AS TotalProducts,
    0 AS TotalOrders -- Orders table doesn't exist yet
FROM Companies c
LEFT JOIN Products p ON p.IsActive = 1
GROUP BY c.Id, c.Name;
GO

-- Supplier Portal View  
CREATE OR ALTER VIEW Portal.SupplierDashboardData
AS
SELECT 
    c.Id AS CompanyId,
    c.Name AS CompanyName,
    COUNT(DISTINCT p.Id) AS MyProducts,
    0 AS ReceivedOrders -- Orders table doesn't have SupplierId yet
FROM Companies c
LEFT JOIN Products p ON p.IsActive = 1
GROUP BY c.Id, c.Name;
GO

PRINT 'Portal views created successfully!';

-- List all schemas
SELECT 
    s.name AS SchemaName,
    s.schema_id,
    p.name AS Owner
FROM sys.schemas s
LEFT JOIN sys.database_principals p ON s.principal_id = p.principal_id
WHERE s.name IN ('Core', 'Trading', 'Analytics', 'Portal', 'dbo')
ORDER BY s.name;
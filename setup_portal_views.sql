-- Check current user and database
SELECT SUSER_NAME() AS CurrentUser, DB_NAME() AS CurrentDatabase;
GO

-- List all schemas
SELECT 
    s.name AS SchemaName,
    s.schema_id,
    p.name AS Owner
FROM sys.schemas s
LEFT JOIN sys.database_principals p ON s.principal_id = p.principal_id
WHERE s.name IN ('Core', 'Trading', 'Analytics', 'Portal', 'dbo')
ORDER BY s.name;
GO

-- Buyer Portal View
CREATE OR ALTER VIEW Portal.BuyerDashboardData
AS
SELECT 
    c.Id AS CompanyId,
    c.Name AS CompanyName,
    COUNT(DISTINCT p.Id) AS TotalProducts
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
    COUNT(DISTINCT p.Id) AS TotalProducts
FROM Companies c
LEFT JOIN Products p ON p.IsActive = 1
GROUP BY c.Id, c.Name;
GO

PRINT 'Views created successfully!';
-- Database Performance Optimization Script
-- Created: January 20, 2025
-- Purpose: Add indexes and optimize queries for FoodX database

-- =====================================================
-- 1. INDEXES FOR FOODXBUYERS TABLE
-- =====================================================

-- Index for company name searches
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodXBuyers_Company')
CREATE NONCLUSTERED INDEX IX_FoodXBuyers_Company 
ON FoodXBuyers(Company) 
INCLUDE (Type, Region, ProcurementEmail, ContactEmail);

-- Index for type filtering
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodXBuyers_Type')
CREATE NONCLUSTERED INDEX IX_FoodXBuyers_Type 
ON FoodXBuyers(Type);

-- Index for region filtering
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodXBuyers_Region')
CREATE NONCLUSTERED INDEX IX_FoodXBuyers_Region 
ON FoodXBuyers(Region);

-- Composite index for common search patterns
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodXBuyers_TypeRegion')
CREATE NONCLUSTERED INDEX IX_FoodXBuyers_TypeRegion 
ON FoodXBuyers(Type, Region) 
INCLUDE (Company, ProcurementEmail);

-- =====================================================
-- 2. INDEXES FOR FOODXSUPPLIERS TABLE
-- =====================================================

-- Index for supplier name searches
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodXSuppliers_SupplierName')
CREATE NONCLUSTERED INDEX IX_FoodXSuppliers_SupplierName 
ON FoodXSuppliers(SupplierName) 
INCLUDE (ProductCategory, Country, CompanyEmail);

-- Index for product category filtering
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodXSuppliers_ProductCategory')
CREATE NONCLUSTERED INDEX IX_FoodXSuppliers_ProductCategory 
ON FoodXSuppliers(ProductCategory);

-- Index for country filtering
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodXSuppliers_Country')
CREATE NONCLUSTERED INDEX IX_FoodXSuppliers_Country 
ON FoodXSuppliers(Country);

-- Composite index for category and country
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodXSuppliers_CategoryCountry')
CREATE NONCLUSTERED INDEX IX_FoodXSuppliers_CategoryCountry 
ON FoodXSuppliers(ProductCategory, Country) 
INCLUDE (SupplierName, CompanyEmail, CompanyWebsite);

-- =====================================================
-- 3. INDEXES FOR COMPANIES TABLE
-- =====================================================

-- Index for company type filtering
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Companies_CompanyType')
CREATE NONCLUSTERED INDEX IX_Companies_CompanyType 
ON Companies(CompanyType) 
WHERE IsActive = 1;

-- Index for company name searches
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Companies_Name')
CREATE NONCLUSTERED INDEX IX_Companies_Name 
ON Companies(Name) 
INCLUDE (CompanyType, MainEmail, Country);

-- Index for active companies
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Companies_IsActive')
CREATE NONCLUSTERED INDEX IX_Companies_IsActive 
ON Companies(IsActive) 
INCLUDE (Name, CompanyType);

-- =====================================================
-- 4. INDEXES FOR PRODUCTS TABLE
-- =====================================================

-- Index for product category
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_Category')
CREATE NONCLUSTERED INDEX IX_Products_Category 
ON Products(Category) 
WHERE IsActive = 1;

-- Index for product availability
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_IsAvailable')
CREATE NONCLUSTERED INDEX IX_Products_IsAvailable 
ON Products(IsAvailable) 
INCLUDE (Name, Category, Price);

-- Composite index for category and availability
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_CategoryAvailable')
CREATE NONCLUSTERED INDEX IX_Products_CategoryAvailable 
ON Products(Category, IsAvailable) 
INCLUDE (Name, Price, Unit)
WHERE IsActive = 1;

-- =====================================================
-- 5. INDEXES FOR ASPNETUSERS TABLE
-- =====================================================

-- Index for email lookups (if not exists)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUsers_Email')
CREATE NONCLUSTERED INDEX IX_AspNetUsers_Email 
ON AspNetUsers(Email) 
INCLUDE (FirstName, LastName, CompanyName);

-- Index for normalized email
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUsers_NormalizedEmail')
CREATE NONCLUSTERED INDEX IX_AspNetUsers_NormalizedEmail 
ON AspNetUsers(NormalizedEmail);

-- Index for last login tracking
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUsers_LastLoginAt')
CREATE NONCLUSTERED INDEX IX_AspNetUsers_LastLoginAt 
ON AspNetUsers(LastLoginAt DESC) 
INCLUDE (Email, FirstName, LastName);

-- =====================================================
-- 6. UPDATE STATISTICS
-- =====================================================

-- Update statistics for better query optimization
UPDATE STATISTICS FoodXBuyers WITH FULLSCAN;
UPDATE STATISTICS FoodXSuppliers WITH FULLSCAN;
UPDATE STATISTICS Companies WITH FULLSCAN;
UPDATE STATISTICS Products WITH FULLSCAN;
UPDATE STATISTICS AspNetUsers WITH FULLSCAN;

-- =====================================================
-- 7. REBUILD FRAGMENTED INDEXES
-- =====================================================

-- Check and rebuild fragmented indexes
DECLARE @TableName NVARCHAR(255);
DECLARE @IndexName NVARCHAR(255);
DECLARE @SQL NVARCHAR(MAX);

DECLARE index_cursor CURSOR FOR
SELECT 
    OBJECT_NAME(ips.object_id) AS TableName,
    si.name AS IndexName
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
INNER JOIN sys.indexes si ON ips.object_id = si.object_id AND ips.index_id = si.index_id
WHERE ips.avg_fragmentation_in_percent > 30
    AND ips.index_id > 0
    AND ips.page_count > 1000;

OPEN index_cursor;
FETCH NEXT FROM index_cursor INTO @TableName, @IndexName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SQL = 'ALTER INDEX ' + @IndexName + ' ON ' + @TableName + ' REBUILD;';
    EXEC sp_executesql @SQL;
    PRINT 'Rebuilt index: ' + @IndexName + ' on table: ' + @TableName;
    FETCH NEXT FROM index_cursor INTO @TableName, @IndexName;
END;

CLOSE index_cursor;
DEALLOCATE index_cursor;

-- =====================================================
-- 8. CLEAN UP AND OPTIMIZE
-- =====================================================

-- Clean up old data and optimize storage
DBCC UPDATEUSAGE(0);

-- Display index usage stats
SELECT 
    OBJECT_NAME(s.object_id) AS TableName,
    i.name AS IndexName,
    s.user_seeks,
    s.user_scans,
    s.user_lookups,
    s.user_updates
FROM sys.dm_db_index_usage_stats s
INNER JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id
WHERE s.database_id = DB_ID()
    AND OBJECT_NAME(s.object_id) IN ('FoodXBuyers', 'FoodXSuppliers', 'Companies', 'Products', 'AspNetUsers')
ORDER BY TableName, IndexName;

PRINT 'Database optimization completed successfully!';
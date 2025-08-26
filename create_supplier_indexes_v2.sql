-- Create indexes for FoodXSuppliers table to improve search performance
-- Version 2: Without computed columns (simpler approach)
-- Author: Claude Code
-- Date: 2025-08-26

-- Check if indexes already exist and drop them
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodXSuppliers_SupplierName')
    DROP INDEX IX_FoodXSuppliers_SupplierName ON FoodXSuppliers;

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodXSuppliers_Country')
    DROP INDEX IX_FoodXSuppliers_Country ON FoodXSuppliers;

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodXSuppliers_ProductCategory')
    DROP INDEX IX_FoodXSuppliers_ProductCategory ON FoodXSuppliers;

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodXSuppliers_Search')
    DROP INDEX IX_FoodXSuppliers_Search ON FoodXSuppliers;

-- Index for supplier name searches (most common)
CREATE NONCLUSTERED INDEX IX_FoodXSuppliers_SupplierName 
ON FoodXSuppliers(SupplierName)
INCLUDE (Country, ProductCategory, CompanyEmail);

-- Index for country filtering
CREATE NONCLUSTERED INDEX IX_FoodXSuppliers_Country 
ON FoodXSuppliers(Country)
WHERE Country IS NOT NULL;

-- Index for category filtering  
CREATE NONCLUSTERED INDEX IX_FoodXSuppliers_ProductCategory
ON FoodXSuppliers(ProductCategory)
WHERE ProductCategory IS NOT NULL;

-- Composite index for common search patterns
CREATE NONCLUSTERED INDEX IX_FoodXSuppliers_Search
ON FoodXSuppliers(Country, ProductCategory)
INCLUDE (SupplierName, CompanyEmail);

-- Update statistics for better query plans
UPDATE STATISTICS FoodXSuppliers;

PRINT 'Supplier indexes created successfully';

-- Check index status
SELECT 
    i.name AS IndexName,
    i.type_desc AS IndexType,
    OBJECT_NAME(i.object_id) AS TableName
FROM sys.indexes i
WHERE OBJECT_NAME(i.object_id) = 'FoodXSuppliers'
AND i.name LIKE 'IX_%'
ORDER BY i.name;
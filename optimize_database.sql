-- =====================================================
-- DATABASE OPTIMIZATION AND CLEANUP SCRIPT
-- =====================================================

PRINT 'Starting database optimization...';
PRINT '';

-- 1. Find and remove duplicate suppliers
PRINT '1. REMOVING DUPLICATE SUPPLIERS...';
PRINT '   Finding duplicates by SupplierName...';

-- Create temp table with duplicates info
WITH DuplicateSuppliers AS (
    SELECT 
        Id,
        SupplierName,
        CompanyEmail,
        Country,
        ROW_NUMBER() OVER (
            PARTITION BY SupplierName 
            ORDER BY 
                CASE WHEN CompanyEmail IS NOT NULL THEN 0 ELSE 1 END,
                CASE WHEN Country IS NOT NULL THEN 0 ELSE 1 END,
                CASE WHEN Description IS NOT NULL THEN 0 ELSE 1 END,
                Id
        ) AS RowNum
    FROM FoodXSuppliers
)
-- Delete duplicates, keeping the best record (most complete data)
DELETE FROM FoodXSuppliers
WHERE Id IN (
    SELECT Id 
    FROM DuplicateSuppliers 
    WHERE RowNum > 1
);

PRINT '   Duplicates removed.';

-- 2. Clean up empty or invalid records
PRINT '';
PRINT '2. CLEANING INVALID RECORDS...';

-- Remove suppliers with no name
DELETE FROM FoodXSuppliers
WHERE SupplierName IS NULL OR SupplierName = '' OR SupplierName = 'nan';

-- Remove buyers with no company name
DELETE FROM FoodXBuyers
WHERE Company IS NULL OR Company = '' OR Company = 'nan';

PRINT '   Invalid records removed.';

-- 3. Standardize data
PRINT '';
PRINT '3. STANDARDIZING DATA...';

-- Trim whitespace from key fields
UPDATE FoodXSuppliers
SET 
    SupplierName = LTRIM(RTRIM(SupplierName)),
    Country = LTRIM(RTRIM(Country)),
    CompanyEmail = LOWER(LTRIM(RTRIM(CompanyEmail)))
WHERE SupplierName LIKE ' %' OR SupplierName LIKE '% '
    OR Country LIKE ' %' OR Country LIKE '% '
    OR CompanyEmail LIKE ' %' OR CompanyEmail LIKE '% ';

UPDATE FoodXBuyers
SET 
    Company = LTRIM(RTRIM(Company)),
    Region = LTRIM(RTRIM(Region)),
    ProcurementEmail = LOWER(LTRIM(RTRIM(ProcurementEmail)))
WHERE Company LIKE ' %' OR Company LIKE '% '
    OR Region LIKE ' %' OR Region LIKE '% '
    OR ProcurementEmail LIKE ' %' OR ProcurementEmail LIKE '% ';

-- Fix 'nan' values
UPDATE FoodXSuppliers SET CompanyEmail = NULL WHERE CompanyEmail = 'nan';
UPDATE FoodXSuppliers SET Phone = NULL WHERE Phone = 'nan';
UPDATE FoodXSuppliers SET Country = NULL WHERE Country = 'nan';

UPDATE FoodXBuyers SET ProcurementEmail = NULL WHERE ProcurementEmail = 'nan';
UPDATE FoodXBuyers SET ProcurementPhone = NULL WHERE ProcurementPhone = 'nan';

PRINT '   Data standardized.';

-- 4. Add missing indexes for performance
PRINT '';
PRINT '4. OPTIMIZING INDEXES...';

-- Check and create indexes if they don't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodXSuppliers_SupplierName_Unique')
    CREATE UNIQUE INDEX IX_FoodXSuppliers_SupplierName_Unique ON FoodXSuppliers(SupplierName);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodXSuppliers_CompanyEmail')
    CREATE INDEX IX_FoodXSuppliers_CompanyEmail ON FoodXSuppliers(CompanyEmail);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodXBuyers_Company_Unique')
    CREATE UNIQUE INDEX IX_FoodXBuyers_Company_Unique ON FoodXBuyers(Company);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodXBuyers_ProcurementEmail')
    CREATE INDEX IX_FoodXBuyers_ProcurementEmail ON FoodXBuyers(ProcurementEmail);

PRINT '   Indexes optimized.';

-- 5. Update statistics
PRINT '';
PRINT '5. UPDATING STATISTICS...';

UPDATE STATISTICS FoodXSuppliers;
UPDATE STATISTICS FoodXBuyers;
UPDATE STATISTICS Exhibitors;
UPDATE STATISTICS Exhibitions;

PRINT '   Statistics updated.';

-- 6. Report final counts
PRINT '';
PRINT '========================================';
PRINT 'OPTIMIZATION COMPLETE - FINAL COUNTS:';
PRINT '========================================';

DECLARE @BuyerCount INT = (SELECT COUNT(*) FROM FoodXBuyers);
DECLARE @SupplierCount INT = (SELECT COUNT(*) FROM FoodXSuppliers);
DECLARE @UniqueCountries INT = (SELECT COUNT(DISTINCT Country) FROM FoodXSuppliers WHERE Country IS NOT NULL);
DECLARE @SuppliersWithEmail INT = (SELECT COUNT(*) FROM FoodXSuppliers WHERE CompanyEmail IS NOT NULL);
DECLARE @BuyersWithEmail INT = (SELECT COUNT(*) FROM FoodXBuyers WHERE ProcurementEmail IS NOT NULL);

PRINT 'FoodXBuyers: ' + CAST(@BuyerCount AS VARCHAR(10)) + ' records';
PRINT '  - With procurement email: ' + CAST(@BuyersWithEmail AS VARCHAR(10));
PRINT '';
PRINT 'FoodXSuppliers: ' + CAST(@SupplierCount AS VARCHAR(10)) + ' records';
PRINT '  - With company email: ' + CAST(@SuppliersWithEmail AS VARCHAR(10));
PRINT '  - Unique countries: ' + CAST(@UniqueCountries AS VARCHAR(10));

PRINT '';
PRINT 'Database optimization completed successfully!';
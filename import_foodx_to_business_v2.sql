-- Import FoodX Data to Business Tables
-- This script migrates data from FoodX import tables to the main business tables

-- ============================================
-- 1. IMPORT COMPANIES FROM FOODX DATA
-- ============================================

-- Clear existing test data (be careful in production!)
DELETE FROM Products WHERE CompanyId IN (SELECT Id FROM Companies WHERE Name LIKE 'TEST_%');
DELETE FROM Suppliers WHERE CompanyId IN (SELECT Id FROM Companies WHERE Name LIKE 'TEST_%');
DELETE FROM Buyers WHERE CompanyId IN (SELECT Id FROM Companies WHERE Name LIKE 'TEST_%');
DELETE FROM Companies WHERE Name LIKE 'TEST_%';

-- Import Buyer Companies (limit to 100 for initial import)
INSERT INTO Companies (Id, Name, CompanyType, Website, Email, Phone, Address, City, Country, PostalCode, Description, IsActive, CreatedAt, UpdatedAt)
SELECT TOP 100
    NEWID() as Id,
    Company as Name,
    'Buyer' as CompanyType,
    NULL as Website,
    ProcurementEmail as Email,
    ContactNumber as Phone,
    NULL as Address,
    Region as City,
    CASE 
        WHEN Region = 'Israel' THEN 'IL'
        WHEN Region LIKE '%USA%' OR Region LIKE '%United States%' THEN 'US'
        WHEN Region LIKE '%UK%' OR Region LIKE '%United Kingdom%' THEN 'GB'
        WHEN Region LIKE '%Germany%' THEN 'DE'
        WHEN Region LIKE '%France%' THEN 'FR'
        WHEN Region LIKE '%Italy%' THEN 'IT'
        WHEN Region LIKE '%Spain%' THEN 'ES'
        WHEN Region LIKE '%Netherlands%' THEN 'NL'
        ELSE 'IL'
    END as Country,
    NULL as PostalCode,
    CONCAT('Type: ', Type, ' | Markets: ', Markets) as Description,
    1 as IsActive,
    GETUTCDATE() as CreatedAt,
    GETUTCDATE() as UpdatedAt
FROM FoodXBuyers
WHERE Company IS NOT NULL 
AND Company != ''
AND NOT EXISTS (SELECT 1 FROM Companies WHERE Name = FoodXBuyers.Company)
ORDER BY Company;

-- Import Supplier Companies (limit to 200 for initial import)
INSERT INTO Companies (Id, Name, CompanyType, Website, Email, Phone, Address, City, Country, PostalCode, Description, IsActive, CreatedAt, UpdatedAt)
SELECT TOP 200
    NEWID() as Id,
    SupplierName as Name,
    'Supplier' as CompanyType,
    CompanyWebsite as Website,
    CompanyEmail as Email,
    ContactNumber as Phone,
    CompanyAddress as Address,
    City,
    CASE 
        WHEN Country IS NULL OR Country = '' THEN 'US'
        WHEN LEN(Country) = 2 THEN Country
        WHEN Country = 'United States' OR Country = 'USA' THEN 'US'
        WHEN Country = 'United Kingdom' OR Country = 'UK' THEN 'GB'
        WHEN Country = 'Germany' THEN 'DE'
        WHEN Country = 'France' THEN 'FR'
        WHEN Country = 'Italy' THEN 'IT'
        WHEN Country = 'Spain' THEN 'ES'
        WHEN Country = 'Netherlands' THEN 'NL'
        WHEN Country = 'Canada' THEN 'CA'
        WHEN Country = 'Israel' THEN 'IL'
        ELSE 'US'
    END as Country,
    PostalCode,
    SUBSTRING(CONCAT('Category: ', ISNULL(ProductCategory, 'General'), ' | ', ISNULL(Description, '')), 1, 500) as Description,
    1 as IsActive,
    GETUTCDATE() as CreatedAt,
    GETUTCDATE() as UpdatedAt
FROM FoodXSuppliers
WHERE SupplierName IS NOT NULL 
AND SupplierName != ''
AND NOT EXISTS (SELECT 1 FROM Companies WHERE Name = FoodXSuppliers.SupplierName)
ORDER BY 
    CASE WHEN Country IS NOT NULL THEN 0 ELSE 1 END,
    CASE WHEN ProductCategory IS NOT NULL THEN 0 ELSE 1 END,
    Id;

-- ============================================
-- 2. CREATE BUYER RECORDS
-- ============================================

-- Link buyer companies to Buyers table
INSERT INTO Buyers (Id, UserId, CompanyId, BuyerType, PurchasingVolume, PreferredCategories, CreatedAt)
SELECT 
    NEWID() as Id,
    CASE ((ROW_NUMBER() OVER (ORDER BY c.Name) - 1) % 3)
        WHEN 0 THEN (SELECT TOP 1 Id FROM AspNetUsers WHERE Email = 'buyer1@test.fdx.trading')
        WHEN 1 THEN (SELECT TOP 1 Id FROM AspNetUsers WHERE Email = 'buyer2@test.fdx.trading')
        ELSE (SELECT TOP 1 Id FROM AspNetUsers WHERE Email = 'buyer3@test.fdx.trading')
    END as UserId,
    c.Id as CompanyId,
    'Retailer' as BuyerType,
    'Medium' as PurchasingVolume,
    'Food & Beverage' as PreferredCategories,
    GETUTCDATE() as CreatedAt
FROM Companies c
WHERE c.CompanyType = 'Buyer'
AND NOT EXISTS (SELECT 1 FROM Buyers WHERE CompanyId = c.Id);

-- ============================================
-- 3. CREATE SUPPLIER RECORDS
-- ============================================

-- Link supplier companies to Suppliers table
INSERT INTO Suppliers (Id, UserId, CompanyId, SupplierType, ProductCategories, CertificationLevel, Rating, CreatedAt)
SELECT 
    NEWID() as Id,
    CASE ((ROW_NUMBER() OVER (ORDER BY c.Name) - 1) % 3)
        WHEN 0 THEN (SELECT TOP 1 Id FROM AspNetUsers WHERE Email = 'supplier1@test.fdx.trading')
        WHEN 1 THEN (SELECT TOP 1 Id FROM AspNetUsers WHERE Email = 'supplier2@test.fdx.trading')
        ELSE (SELECT TOP 1 Id FROM AspNetUsers WHERE Email = 'supplier3@test.fdx.trading')
    END as UserId,
    c.Id as CompanyId,
    'Manufacturer' as SupplierType,
    SUBSTRING(ISNULL(c.Description, 'General'), 1, 100) as ProductCategories,
    'Basic' as CertificationLevel,
    4.0 as Rating,
    GETUTCDATE() as CreatedAt
FROM Companies c
WHERE c.CompanyType = 'Supplier'
AND NOT EXISTS (SELECT 1 FROM Suppliers WHERE CompanyId = c.Id);

-- ============================================
-- 4. CREATE SAMPLE PRODUCTS
-- ============================================

-- Create products for suppliers (limit to 500 for initial import)
INSERT INTO Products (
    Id, Name, Description, Category, SKU, Price, Currency, Unit, 
    MinOrderQuantity, StockQuantity, LeadTime, Origin, 
    SupplierId, CompanyId, IsOrganic, IsAvailable, CreatedAt, UpdatedAt
)
SELECT TOP 500
    NEWID() as Id,
    CONCAT(
        CASE (ABS(CHECKSUM(NEWID())) % 10)
            WHEN 0 THEN 'Premium '
            WHEN 1 THEN 'Organic '
            WHEN 2 THEN 'Fresh '
            WHEN 3 THEN 'Frozen '
            WHEN 4 THEN 'Dried '
            WHEN 5 THEN 'Canned '
            WHEN 6 THEN 'Specialty '
            WHEN 7 THEN 'Gourmet '
            WHEN 8 THEN 'Natural '
            ELSE 'Quality '
        END,
        CASE (ABS(CHECKSUM(NEWID())) % 15)
            WHEN 0 THEN 'Tomatoes'
            WHEN 1 THEN 'Potatoes'
            WHEN 2 THEN 'Onions'
            WHEN 3 THEN 'Carrots'
            WHEN 4 THEN 'Apples'
            WHEN 5 THEN 'Oranges'
            WHEN 6 THEN 'Chicken Breast'
            WHEN 7 THEN 'Beef Cuts'
            WHEN 8 THEN 'Salmon Fillet'
            WHEN 9 THEN 'Olive Oil'
            WHEN 10 THEN 'Pasta'
            WHEN 11 THEN 'Rice'
            WHEN 12 THEN 'Bread'
            WHEN 13 THEN 'Cheese'
            ELSE 'Mixed Vegetables'
        END
    ) as Name,
    'High quality product sourced from verified suppliers' as Description,
    CASE (ABS(CHECKSUM(NEWID())) % 8)
        WHEN 0 THEN 'Fruits'
        WHEN 1 THEN 'Vegetables'
        WHEN 2 THEN 'Meat'
        WHEN 3 THEN 'Seafood'
        WHEN 4 THEN 'Dairy'
        WHEN 5 THEN 'Grains'
        WHEN 6 THEN 'Oils'
        ELSE 'Beverages'
    END as Category,
    CONCAT('SKU-', LEFT(REPLACE(CAST(NEWID() AS VARCHAR(36)), '-', ''), 8)) as SKU,
    CAST((ABS(CHECKSUM(NEWID())) % 10000 + 100) / 100.0 as DECIMAL(10,2)) as Price,
    'USD' as Currency,
    CASE (ABS(CHECKSUM(NEWID())) % 4)
        WHEN 0 THEN 'KG'
        WHEN 1 THEN 'LB'
        WHEN 2 THEN 'CASE'
        ELSE 'UNIT'
    END as Unit,
    CASE (ABS(CHECKSUM(NEWID())) % 4)
        WHEN 0 THEN 10
        WHEN 1 THEN 25
        WHEN 2 THEN 50
        ELSE 100
    END as MinOrderQuantity,
    ABS(CHECKSUM(NEWID())) % 10000 + 100 as StockQuantity,
    CASE (ABS(CHECKSUM(NEWID())) % 4)
        WHEN 0 THEN '1-3 days'
        WHEN 1 THEN '3-5 days'
        WHEN 2 THEN '1 week'
        ELSE '2 weeks'
    END as LeadTime,
    c.Country as Origin,
    s.Id as SupplierId,
    c.Id as CompanyId,
    CASE WHEN ABS(CHECKSUM(NEWID())) % 5 = 0 THEN 1 ELSE 0 END as IsOrganic,
    1 as IsAvailable,
    GETUTCDATE() as CreatedAt,
    GETUTCDATE() as UpdatedAt
FROM Suppliers s
INNER JOIN Companies c ON s.CompanyId = c.Id
WHERE c.CompanyType = 'Supplier'
ORDER BY NEWID();

-- ============================================
-- 5. VERIFICATION QUERIES
-- ============================================

-- Summary counts
SELECT 'Companies' as TableName, COUNT(*) as RecordCount FROM Companies
UNION ALL
SELECT 'Buyers', COUNT(*) FROM Buyers
UNION ALL
SELECT 'Suppliers', COUNT(*) FROM Suppliers
UNION ALL
SELECT 'Products', COUNT(*) FROM Products
ORDER BY TableName;

-- Company breakdown
SELECT CompanyType, COUNT(*) as Count 
FROM Companies 
GROUP BY CompanyType
ORDER BY CompanyType;

-- Products by category
SELECT Category, COUNT(*) as Count 
FROM Products 
GROUP BY Category
ORDER BY Count DESC;

-- Top supplier companies with products
SELECT TOP 10 
    c.Name,
    c.Country,
    COUNT(p.Id) as ProductCount
FROM Companies c
LEFT JOIN Products p ON c.Id = p.CompanyId
WHERE c.CompanyType = 'Supplier'
GROUP BY c.Name, c.Country
ORDER BY ProductCount DESC;
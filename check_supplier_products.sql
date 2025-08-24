-- =====================================================
-- SUPPLIER PRODUCTS ANALYSIS
-- =====================================================

PRINT 'SUPPLIER PRODUCTS DATA ANALYSIS';
PRINT '================================';
PRINT '';

-- 1. Product columns overview
PRINT '1. PRODUCT DATA AVAILABILITY:';
DECLARE @TotalSuppliers INT = (SELECT COUNT(*) FROM FoodXSuppliers);
DECLARE @WithProducts INT = (SELECT COUNT(*) FROM FoodXSuppliers WHERE Products IS NOT NULL AND Products != '');
DECLARE @WithCategory INT = (SELECT COUNT(*) FROM FoodXSuppliers WHERE ProductCategory IS NOT NULL AND ProductCategory != '');
DECLARE @WithDescription INT = (SELECT COUNT(*) FROM FoodXSuppliers WHERE Description IS NOT NULL AND Description != '');

PRINT '  Total Suppliers: ' + CAST(@TotalSuppliers AS VARCHAR(10));
PRINT '  With Products field: ' + CAST(@WithProducts AS VARCHAR(10)) + ' (' + CAST(@WithProducts*100/@TotalSuppliers AS VARCHAR(10)) + '%)';
PRINT '  With ProductCategory: ' + CAST(@WithCategory AS VARCHAR(10)) + ' (' + CAST(@WithCategory*100/@TotalSuppliers AS VARCHAR(10)) + '%)';
PRINT '  With Description: ' + CAST(@WithDescription AS VARCHAR(10)) + ' (' + CAST(@WithDescription*100/@TotalSuppliers AS VARCHAR(10)) + '%)';
PRINT '';

-- 2. Sample suppliers with products
PRINT '2. SAMPLE SUPPLIERS WITH PRODUCT DATA:';
PRINT '---------------------------------------';
SELECT TOP 10
    SupplierName,
    ISNULL(Country, 'N/A') as Country,
    CASE 
        WHEN LEN(ISNULL(ProductCategory, '')) > 50 
        THEN LEFT(ProductCategory, 50) + '...'
        ELSE ISNULL(ProductCategory, 'N/A')
    END as Category,
    CASE 
        WHEN Products IS NOT NULL AND Products != ''
        THEN 'Yes (' + CAST(LEN(Products) AS VARCHAR(10)) + ' chars)'
        ELSE 'No'
    END as HasProducts
FROM FoodXSuppliers
WHERE Products IS NOT NULL AND Products != ''
   OR ProductCategory IS NOT NULL AND ProductCategory != ''
ORDER BY SupplierName;

PRINT '';

-- 3. Top product categories
PRINT '3. TOP 20 PRODUCT CATEGORIES:';
PRINT '------------------------------';
SELECT TOP 20
    CASE 
        WHEN LEN(ISNULL(ProductCategory, 'Not Specified')) > 60
        THEN LEFT(ProductCategory, 60) + '...'
        ELSE ISNULL(ProductCategory, 'Not Specified')
    END as Category,
    COUNT(*) as SupplierCount
FROM FoodXSuppliers
WHERE ProductCategory IS NOT NULL AND ProductCategory != ''
GROUP BY ProductCategory
ORDER BY COUNT(*) DESC;

PRINT '';

-- 4. Search for specific product types
PRINT '4. SUPPLIERS BY PRODUCT TYPE (Examples):';
PRINT '-----------------------------------------';

-- Meat suppliers
PRINT '';
PRINT 'MEAT SUPPLIERS:';
SELECT TOP 5
    SupplierName,
    ISNULL(Country, 'N/A') as Country,
    CASE 
        WHEN LEN(ISNULL(ProductCategory, '')) > 50
        THEN LEFT(ProductCategory, 50) + '...'
        ELSE ISNULL(ProductCategory, 'N/A')
    END as Category
FROM FoodXSuppliers
WHERE Products LIKE '%meat%' 
   OR Description LIKE '%meat%'
   OR ProductCategory LIKE '%meat%'
ORDER BY SupplierName;

-- Dairy suppliers
PRINT '';
PRINT 'DAIRY SUPPLIERS:';
SELECT TOP 5
    SupplierName,
    ISNULL(Country, 'N/A') as Country,
    CASE 
        WHEN LEN(ISNULL(ProductCategory, '')) > 50
        THEN LEFT(ProductCategory, 50) + '...'
        ELSE ISNULL(ProductCategory, 'N/A')
    END as Category
FROM FoodXSuppliers
WHERE Products LIKE '%dairy%' 
   OR Description LIKE '%dairy%'
   OR ProductCategory LIKE '%dairy%'
   OR Products LIKE '%milk%'
   OR Description LIKE '%milk%'
   OR Products LIKE '%cheese%'
   OR Description LIKE '%cheese%'
ORDER BY SupplierName;

-- Fruit/Vegetable suppliers
PRINT '';
PRINT 'FRUIT/VEGETABLE SUPPLIERS:';
SELECT TOP 5
    SupplierName,
    ISNULL(Country, 'N/A') as Country,
    CASE 
        WHEN LEN(ISNULL(ProductCategory, '')) > 50
        THEN LEFT(ProductCategory, 50) + '...'
        ELSE ISNULL(ProductCategory, 'N/A')
    END as Category
FROM FoodXSuppliers
WHERE Products LIKE '%fruit%' 
   OR Description LIKE '%fruit%'
   OR ProductCategory LIKE '%fruit%'
   OR Products LIKE '%vegetable%'
   OR Description LIKE '%vegetable%'
   OR ProductCategory LIKE '%produce%'
ORDER BY SupplierName;

-- Beverage suppliers
PRINT '';
PRINT 'BEVERAGE SUPPLIERS:';
SELECT TOP 5
    SupplierName,
    ISNULL(Country, 'N/A') as Country,
    CASE 
        WHEN LEN(ISNULL(ProductCategory, '')) > 50
        THEN LEFT(ProductCategory, 50) + '...'
        ELSE ISNULL(ProductCategory, 'N/A')
    END as Category
FROM FoodXSuppliers
WHERE Products LIKE '%beverage%' 
   OR Description LIKE '%beverage%'
   OR ProductCategory LIKE '%beverage%'
   OR Products LIKE '%drink%'
   OR Description LIKE '%drink%'
   OR Products LIKE '%juice%'
   OR Description LIKE '%juice%'
ORDER BY SupplierName;

PRINT '';

-- 5. Product data by country
PRINT '5. PRODUCT DATA COMPLETENESS BY COUNTRY:';
PRINT '-----------------------------------------';
SELECT TOP 15
    ISNULL(Country, 'Not Specified') as Country,
    COUNT(*) as TotalSuppliers,
    SUM(CASE WHEN Products IS NOT NULL AND Products != '' THEN 1 ELSE 0 END) as WithProducts,
    SUM(CASE WHEN ProductCategory IS NOT NULL AND ProductCategory != '' THEN 1 ELSE 0 END) as WithCategory,
    SUM(CASE WHEN Description IS NOT NULL AND Description != '' THEN 1 ELSE 0 END) as WithDescription
FROM FoodXSuppliers
GROUP BY Country
HAVING COUNT(*) > 10
ORDER BY COUNT(*) DESC;

PRINT '';
PRINT '================================';
PRINT 'ANALYSIS COMPLETE';
PRINT '================================';
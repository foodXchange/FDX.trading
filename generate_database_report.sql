-- =====================================================
-- FoodX Trading Platform - Database Summary Report
-- =====================================================

PRINT '';
PRINT '========================================';
PRINT 'FOODX DATABASE SUMMARY REPORT';
PRINT '========================================';
PRINT '';

-- 1. BUYERS SUMMARY
PRINT '1. BUYERS DATA';
PRINT '----------------------------------------';
SELECT 
    COUNT(*) as TotalBuyers,
    COUNT(DISTINCT Country) as Countries,
    COUNT(Email) as WithEmail,
    COUNT(Phone) as WithPhone,
    COUNT(Website) as WithWebsite
FROM FoodXBuyers;

-- Top buyer countries
PRINT '';
PRINT 'Top Buyer Countries:';
SELECT TOP 5
    Country,
    COUNT(*) as BuyerCount
FROM FoodXBuyers
WHERE Country IS NOT NULL
GROUP BY Country
ORDER BY COUNT(*) DESC;

-- 2. SUPPLIERS SUMMARY
PRINT '';
PRINT '2. SUPPLIERS DATA';
PRINT '----------------------------------------';
SELECT 
    COUNT(*) as TotalSuppliers,
    COUNT(DISTINCT Country) as Countries,
    COUNT(ProductsList) as WithProductsList,
    COUNT(BrandsList) as WithBrands,
    COUNT(CASE WHEN KosherCertification = 1 THEN 1 END) as KosherCertified,
    COUNT(OtherCertifications) as WithCertifications,
    COUNT(YearFounded) as WithYearFounded
FROM FoodXSuppliers;

-- Top supplier countries
PRINT '';
PRINT 'Top Supplier Countries:';
SELECT TOP 5
    Country,
    COUNT(*) as SupplierCount
FROM FoodXSuppliers
WHERE Country IS NOT NULL
GROUP BY Country
ORDER BY COUNT(*) DESC;

-- Product categories distribution
PRINT '';
PRINT 'Product Categories:';
SELECT TOP 10
    ProductCategory,
    COUNT(*) as SupplierCount
FROM FoodXSuppliers
WHERE ProductCategory IS NOT NULL
GROUP BY ProductCategory
ORDER BY COUNT(*) DESC;

-- 3. EXHIBITORS SUMMARY
PRINT '';
PRINT '3. EXHIBITORS DATA';
PRINT '----------------------------------------';
SELECT 
    COUNT(*) as TotalExhibitors,
    COUNT(DISTINCT Country) as Countries,
    COUNT(Email) as WithEmail,
    COUNT(Phone) as WithPhone
FROM Exhibitors;

-- 4. DATA QUALITY METRICS
PRINT '';
PRINT '4. DATA QUALITY METRICS';
PRINT '----------------------------------------';
SELECT 
    'Suppliers' as DataType,
    COUNT(*) as TotalRecords,
    COUNT(CASE WHEN CompanyEmail IS NOT NULL OR PrimaryEmail IS NOT NULL THEN 1 END) as WithEmail,
    CAST(COUNT(CASE WHEN CompanyEmail IS NOT NULL OR PrimaryEmail IS NOT NULL THEN 1 END) * 100.0 / COUNT(*) as DECIMAL(5,2)) as EmailPercent,
    COUNT(CASE WHEN Phone IS NOT NULL OR PrimaryPhone IS NOT NULL THEN 1 END) as WithPhone,
    CAST(COUNT(CASE WHEN Phone IS NOT NULL OR PrimaryPhone IS NOT NULL THEN 1 END) * 100.0 / COUNT(*) as DECIMAL(5,2)) as PhonePercent,
    COUNT(CompanyWebsite) as WithWebsite,
    CAST(COUNT(CompanyWebsite) * 100.0 / COUNT(*) as DECIMAL(5,2)) as WebsitePercent
FROM FoodXSuppliers

UNION ALL

SELECT 
    'Buyers' as DataType,
    COUNT(*) as TotalRecords,
    COUNT(Email) as WithEmail,
    CAST(COUNT(Email) * 100.0 / COUNT(*) as DECIMAL(5,2)) as EmailPercent,
    COUNT(Phone) as WithPhone,
    CAST(COUNT(Phone) * 100.0 / COUNT(*) as DECIMAL(5,2)) as PhonePercent,
    COUNT(Website) as WithWebsite,
    CAST(COUNT(Website) * 100.0 / COUNT(*) as DECIMAL(5,2)) as WebsitePercent
FROM FoodXBuyers;

-- 5. ENHANCED DATA COVERAGE
PRINT '';
PRINT '5. ENHANCED DATA COVERAGE';
PRINT '----------------------------------------';
SELECT 
    COUNT(*) as TotalSuppliers,
    COUNT(ProductsList) as WithEnhancedProducts,
    CAST(COUNT(ProductsList) * 100.0 / COUNT(*) as DECIMAL(5,2)) as EnhancedPercent,
    COUNT(CASE WHEN ProductsFound > 0 THEN 1 END) as WithProductsFound,
    COUNT(CASE WHEN BrandsFound > 0 THEN 1 END) as WithBrandsFound,
    COUNT(DataSource) as WithDataSource,
    COUNT(FirstCreated) as WithCreatedDate,
    COUNT(CompanyLogoURL) as WithLogo
FROM FoodXSuppliers;

-- 6. RECENT UPDATES
PRINT '';
PRINT '6. RECENT DATA UPDATES';
PRINT '----------------------------------------';
SELECT TOP 10
    SupplierName,
    LastUpdated,
    ProductsFound,
    BrandsFound,
    DataSource
FROM FoodXSuppliers
WHERE LastUpdated IS NOT NULL
ORDER BY LastUpdated DESC;

PRINT '';
PRINT '========================================';
PRINT 'END OF REPORT';
PRINT '========================================';
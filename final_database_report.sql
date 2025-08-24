-- FINAL DATABASE REPORT
DECLARE @BuyerCount INT = (SELECT COUNT(*) FROM FoodXBuyers);
DECLARE @SupplierCount INT = (SELECT COUNT(DISTINCT SupplierName) FROM FoodXSuppliers);
DECLARE @ExhibitorCount INT = (SELECT COUNT(*) FROM Exhibitors);
DECLARE @ExhibitionCount INT = (SELECT COUNT(*) FROM Exhibitions);
DECLARE @UniqueCountries INT = (SELECT COUNT(DISTINCT Country) FROM FoodXSuppliers WHERE Country IS NOT NULL AND Country != '');
DECLARE @SuppliersWithEmail INT = (SELECT COUNT(*) FROM FoodXSuppliers WHERE CompanyEmail IS NOT NULL AND CompanyEmail != '');
DECLARE @BuyersWithEmail INT = (SELECT COUNT(*) FROM FoodXBuyers WHERE ProcurementEmail IS NOT NULL AND ProcurementEmail != '');

PRINT '========================================';
PRINT 'FINAL OPTIMIZED DATABASE REPORT';
PRINT '========================================';
PRINT '';
PRINT 'BUSINESS ENTITIES:';
PRINT '  FoodX Buyers      : ' + CAST(@BuyerCount AS VARCHAR(10));
PRINT '  FoodX Suppliers   : ' + CAST(@SupplierCount AS VARCHAR(10)) + ' (unique)';
PRINT '  Exhibitors        : ' + CAST(@ExhibitorCount AS VARCHAR(10));
PRINT '  Exhibitions       : ' + CAST(@ExhibitionCount AS VARCHAR(10));
PRINT '';
PRINT 'DATA QUALITY:';
PRINT '  Buyers with email     : ' + CAST(@BuyersWithEmail AS VARCHAR(10)) + ' / ' + CAST(@BuyerCount AS VARCHAR(10));
PRINT '  Suppliers with email  : ' + CAST(@SuppliersWithEmail AS VARCHAR(10)) + ' / ' + CAST(@SupplierCount AS VARCHAR(10));
PRINT '  Unique countries      : ' + CAST(@UniqueCountries AS VARCHAR(10));
PRINT '';

-- Top countries
PRINT 'TOP SUPPLIER COUNTRIES:';
SELECT TOP 10
    ISNULL(Country, 'Not Specified') as Country,
    COUNT(*) as SupplierCount
FROM FoodXSuppliers
GROUP BY Country
ORDER BY COUNT(*) DESC;

-- Top buyer regions
PRINT '';
PRINT 'BUYER REGIONS:';
SELECT 
    ISNULL(Region, 'Not Specified') as Region,
    COUNT(*) as BuyerCount
FROM FoodXBuyers
GROUP BY Region
ORDER BY COUNT(*) DESC;

-- Exhibitions
PRINT '';
PRINT 'EXHIBITIONS:';
SELECT 
    Name,
    (SELECT COUNT(*) FROM Exhibitors WHERE ExhibitionId = Exhibitions.Id) as ExhibitorCount
FROM Exhibitions
ORDER BY Name;
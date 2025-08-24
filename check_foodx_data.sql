-- Check imported data counts
SELECT 'FoodXBuyers' as TableName, COUNT(*) as RecordCount FROM FoodXBuyers
UNION ALL
SELECT 'FoodXSuppliers', COUNT(*) FROM FoodXSuppliers
UNION ALL
SELECT 'Exhibitors', COUNT(*) FROM Exhibitors
UNION ALL
SELECT 'Exhibitions', COUNT(*) FROM Exhibitions
UNION ALL
SELECT 'FoodXProducts', COUNT(*) FROM FoodXProducts
ORDER BY TableName;

-- Sample buyers
PRINT '';
PRINT '=== SAMPLE BUYERS ===';
SELECT TOP 5 
    Id,
    Company,
    Type,
    Region,
    Markets,
    ProcurementEmail
FROM FoodXBuyers
ORDER BY Id;

-- Sample suppliers
PRINT '';
PRINT '=== SAMPLE SUPPLIERS ===';
SELECT TOP 5
    Id,
    SupplierName,
    Country,
    ProductCategory,
    CompanyEmail
FROM FoodXSuppliers
ORDER BY Id;

-- Sample exhibitors
PRINT '';
PRINT '=== SAMPLE EXHIBITORS ===';
SELECT TOP 5
    e.Id,
    e.CompanyName,
    e.Country,
    ex.Name as Exhibition
FROM Exhibitors e
LEFT JOIN Exhibitions ex ON e.ExhibitionId = ex.Id
ORDER BY e.Id;

-- Get statistics
PRINT '';
PRINT '=== STATISTICS ===';
SELECT 
    'Total Buyers' as Metric,
    COUNT(*) as Count
FROM FoodXBuyers
UNION ALL
SELECT 
    'Buyers with Email',
    COUNT(*)
FROM FoodXBuyers
WHERE ProcurementEmail IS NOT NULL AND ProcurementEmail != ''
UNION ALL
SELECT 
    'Total Suppliers',
    COUNT(*)
FROM FoodXSuppliers
UNION ALL
SELECT 
    'Suppliers with Country',
    COUNT(*)
FROM FoodXSuppliers
WHERE Country IS NOT NULL AND Country != ''
UNION ALL
SELECT 
    'Total Exhibitors',
    COUNT(*)
FROM Exhibitors;
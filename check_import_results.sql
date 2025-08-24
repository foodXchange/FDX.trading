-- Check what was imported
SELECT 'Buyers' as TableName, COUNT(*) as Count FROM Buyers
UNION ALL
SELECT 'Suppliers', COUNT(*) FROM Suppliers
UNION ALL
SELECT 'Exhibitors', COUNT(*) FROM Exhibitors
UNION ALL
SELECT 'Exhibitions', COUNT(*) FROM Exhibitions
ORDER BY TableName;

-- Show sample exhibitors
SELECT TOP 5 * FROM Exhibitors;

-- Show exhibitions
SELECT * FROM Exhibitions;
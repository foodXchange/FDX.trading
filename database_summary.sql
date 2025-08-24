-- FoodX Database Summary

-- Total records in each table
SELECT 'FoodXBuyers' as TableName, COUNT(*) as RecordCount FROM FoodXBuyers
UNION ALL
SELECT 'FoodXSuppliers', COUNT(*) FROM FoodXSuppliers
UNION ALL
SELECT 'Exhibitors', COUNT(*) FROM Exhibitors
UNION ALL
SELECT 'AspNetUsers', COUNT(*) FROM AspNetUsers;
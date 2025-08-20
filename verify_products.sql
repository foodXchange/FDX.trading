-- Check if Products table exists
SELECT 
    CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products')
    THEN 'Products table EXISTS'
    ELSE 'Products table NOT FOUND'
    END AS TableStatus;

-- If exists, show count and sample data
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products')
BEGIN
    SELECT COUNT(*) AS TotalProducts FROM Products;
    
    SELECT TOP 5 
        Id, 
        Name, 
        Category, 
        Price, 
        Unit,
        IsAvailable
    FROM Products
    ORDER BY Id;
END
-- Final FoodX Data Import Script
-- Import sample companies and products from FoodX tables

-- ============================================
-- 1. Import Buyer Companies (if not exists)
-- ============================================

DECLARE @MaxId INT = ISNULL((SELECT MAX(Id) FROM Companies), 0);

-- Import top 30 buyer companies
INSERT INTO Companies (Id, Name, CompanyType, BuyerCategory, Country, MainEmail, IsActive, CreatedAt, UpdatedAt)
SELECT 
    @MaxId + ROW_NUMBER() OVER (ORDER BY Company) as Id,
    Company as Name,
    'Buyer' as CompanyType,
    Type as BuyerCategory,
    'IL' as Country, -- Most are from Israel
    CONCAT('buyer', ROW_NUMBER() OVER (ORDER BY Company), '@', REPLACE(LOWER(Company), ' ', ''), '.com') as MainEmail,
    1 as IsActive,
    GETUTCDATE() as CreatedAt,
    GETUTCDATE() as UpdatedAt
FROM (
    SELECT TOP 30 DISTINCT Company, Type
    FROM FoodXBuyers
    WHERE Company IS NOT NULL 
    AND Company != ''
    AND NOT EXISTS (SELECT 1 FROM Companies WHERE Name = FoodXBuyers.Company)
) AS DistinctBuyers;

-- ============================================
-- 2. Import Supplier Companies (if not exists)  
-- ============================================

SET @MaxId = ISNULL((SELECT MAX(Id) FROM Companies), 0);

-- Import top 50 supplier companies
INSERT INTO Companies (Id, Name, CompanyType, Country, Website, MainEmail, IsActive, CreatedAt, UpdatedAt)
SELECT 
    @MaxId + ROW_NUMBER() OVER (ORDER BY SupplierName) as Id,
    SupplierName as Name,
    'Supplier' as CompanyType,
    'US' as Country, -- Default to US
    CompanyWebsite as Website,
    CONCAT('supplier', ROW_NUMBER() OVER (ORDER BY SupplierName), '@', 
           REPLACE(REPLACE(REPLACE(LOWER(SupplierName), ' ', ''), ',', ''), '''', ''), '.com') as MainEmail,
    1 as IsActive,
    GETUTCDATE() as CreatedAt,
    GETUTCDATE() as UpdatedAt
FROM (
    SELECT TOP 50 DISTINCT SupplierName, CompanyWebsite
    FROM FoodXSuppliers
    WHERE SupplierName IS NOT NULL 
    AND SupplierName != ''
    AND LEN(SupplierName) < 100
    AND NOT EXISTS (SELECT 1 FROM Companies WHERE Name = FoodXSuppliers.SupplierName)
) AS DistinctSuppliers;

-- ============================================
-- 3. Create Sample Products
-- ============================================

-- Generate diverse products (not linked to specific suppliers)
DECLARE @ProductId INT = ISNULL((SELECT MAX(Id) FROM Products), 0);

-- Insert 200 sample products
INSERT INTO Products (Id, Name, Category, Description, Price, SKU, Unit, MinOrderQuantity, StockQuantity, Origin, IsOrganic, IsActive, CreatedAt, UpdatedAt)
SELECT 
    @ProductId + Number as Id,
    ProductName as Name,
    Category,
    'High quality product suitable for B2B trading' as Description,
    Price,
    CONCAT('SKU', RIGHT('00000' + CAST(@ProductId + Number AS VARCHAR(5)), 5)) as SKU,
    Unit,
    MinOrder as MinOrderQuantity,
    Stock as StockQuantity,
    Origin,
    IsOrganic,
    1 as IsActive,
    GETUTCDATE() as CreatedAt,
    GETUTCDATE() as UpdatedAt
FROM (
    SELECT TOP 200
        ROW_NUMBER() OVER (ORDER BY NEWID()) as Number,
        CONCAT(
            CASE (ABS(CHECKSUM(NEWID())) % 5)
                WHEN 0 THEN 'Premium '
                WHEN 1 THEN 'Organic '
                WHEN 2 THEN 'Fresh '
                WHEN 3 THEN 'Frozen '
                ELSE ''
            END,
            CASE (ABS(CHECKSUM(NEWID())) % 30)
                WHEN 0 THEN 'Tomatoes'
                WHEN 1 THEN 'Potatoes'
                WHEN 2 THEN 'Onions'
                WHEN 3 THEN 'Carrots'
                WHEN 4 THEN 'Apples'
                WHEN 5 THEN 'Oranges'
                WHEN 6 THEN 'Bananas'
                WHEN 7 THEN 'Strawberries'
                WHEN 8 THEN 'Chicken Breast'
                WHEN 9 THEN 'Beef Steak'
                WHEN 10 THEN 'Salmon Fillet'
                WHEN 11 THEN 'Tuna'
                WHEN 12 THEN 'Olive Oil'
                WHEN 13 THEN 'Sunflower Oil'
                WHEN 14 THEN 'Pasta'
                WHEN 15 THEN 'Rice'
                WHEN 16 THEN 'Bread'
                WHEN 17 THEN 'Cheese'
                WHEN 18 THEN 'Milk'
                WHEN 19 THEN 'Yogurt'
                WHEN 20 THEN 'Eggs'
                WHEN 21 THEN 'Butter'
                WHEN 22 THEN 'Sugar'
                WHEN 23 THEN 'Flour'
                WHEN 24 THEN 'Coffee'
                WHEN 25 THEN 'Tea'
                WHEN 26 THEN 'Chocolate'
                WHEN 27 THEN 'Nuts'
                WHEN 28 THEN 'Honey'
                ELSE 'Mixed Products'
            END
        ) as ProductName,
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
        CAST((ABS(CHECKSUM(NEWID())) % 10000 + 100) / 100.0 as DECIMAL(10,2)) as Price,
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
        END as MinOrder,
        ABS(CHECKSUM(NEWID())) % 10000 + 100 as Stock,
        CASE (ABS(CHECKSUM(NEWID())) % 10)
            WHEN 0 THEN 'US'
            WHEN 1 THEN 'IL'
            WHEN 2 THEN 'ES'
            WHEN 3 THEN 'IT'
            WHEN 4 THEN 'FR'
            WHEN 5 THEN 'NL'
            WHEN 6 THEN 'DE'
            WHEN 7 THEN 'GB'
            WHEN 8 THEN 'CA'
            ELSE 'US'
        END as Origin,
        CASE WHEN ABS(CHECKSUM(NEWID())) % 5 = 0 THEN 1 ELSE 0 END as IsOrganic
    FROM sys.objects -- Just using as a source for row generation
) AS ProductData
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Name = ProductData.ProductName);

-- ============================================
-- 4. Create Users entries for our test accounts
-- ============================================

-- Get max user ID
DECLARE @MaxUserId INT = ISNULL((SELECT MAX(Id) FROM Users), 0);

-- Insert test users into Users table if not exists
INSERT INTO Users (Id, Email, FirstName, LastName, Role, IsActive, CreatedAt, UpdatedAt)
SELECT 
    @MaxUserId + ROW_NUMBER() OVER (ORDER BY u.Email) as Id,
    u.Email,
    u.FirstName,
    u.LastName,
    r.Name as Role,
    1 as IsActive,
    GETUTCDATE() as CreatedAt,
    GETUTCDATE() as UpdatedAt
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email LIKE '%@test.fdx.trading'
AND NOT EXISTS (SELECT 1 FROM Users WHERE Email = u.Email);

-- ============================================
-- 5. Display Import Summary
-- ============================================

-- Count records
SELECT 'Import Summary' as Report;
SELECT '==============' as [==============];

SELECT 'Companies' as Entity, COUNT(*) as Total,
    SUM(CASE WHEN CompanyType = 'Buyer' THEN 1 ELSE 0 END) as Buyers,
    SUM(CASE WHEN CompanyType = 'Supplier' THEN 1 ELSE 0 END) as Suppliers
FROM Companies;

SELECT 'Products' as Entity, COUNT(*) as Total,
    COUNT(DISTINCT Category) as Categories,
    AVG(Price) as AvgPrice
FROM Products;

SELECT 'Users' as Entity, COUNT(*) as Total,
    COUNT(DISTINCT Role) as Roles,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) as Active
FROM Users;

-- Show sample data
SELECT TOP 5 'Sample Companies:' as Info, Name, CompanyType, Country 
FROM Companies 
ORDER BY CreatedAt DESC;

SELECT TOP 5 'Sample Products:' as Info, Name, Category, Price 
FROM Products 
ORDER BY CreatedAt DESC;
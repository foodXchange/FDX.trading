-- Simplified FoodX Data Import
-- Import sample data from FoodX tables to Companies and Products

-- ============================================
-- 1. Import Sample Buyer Companies
-- ============================================

DECLARE @CompanyId INT = (SELECT ISNULL(MAX(Id), 0) FROM Companies) + 1;

-- Import top 50 buyer companies
INSERT INTO Companies (Id, Name, CompanyType, BuyerCategory, Country, MainEmail, IsActive, CreatedAt, UpdatedAt)
SELECT TOP 50
    @CompanyId + ROW_NUMBER() OVER (ORDER BY Company) as Id,
    Company as Name,
    'Buyer' as CompanyType,
    Type as BuyerCategory,
    CASE 
        WHEN Region = 'Israel' THEN 'IL'
        WHEN Region LIKE '%USA%' OR Region LIKE '%United States%' THEN 'US'
        WHEN Region LIKE '%UK%' OR Region LIKE '%United Kingdom%' THEN 'GB'
        ELSE 'IL'
    END as Country,
    ISNULL(ProcurementEmail, CONCAT('info@', REPLACE(LOWER(Company), ' ', ''), '.com')) as MainEmail,
    1 as IsActive,
    GETUTCDATE() as CreatedAt,
    GETUTCDATE() as UpdatedAt
FROM FoodXBuyers
WHERE Company IS NOT NULL 
AND Company != ''
AND NOT EXISTS (SELECT 1 FROM Companies WHERE Name = FoodXBuyers.Company)
ORDER BY Company;

-- ============================================
-- 2. Import Sample Supplier Companies
-- ============================================

SET @CompanyId = (SELECT ISNULL(MAX(Id), 0) FROM Companies) + 1;

-- Import top 100 supplier companies
INSERT INTO Companies (Id, Name, CompanyType, Country, Website, MainEmail, IsActive, CreatedAt, UpdatedAt)
SELECT TOP 100
    @CompanyId + ROW_NUMBER() OVER (ORDER BY SupplierName) as Id,
    SupplierName as Name,
    'Supplier' as CompanyType,
    CASE 
        WHEN Country IS NULL OR Country = '' THEN 'US'
        WHEN Country = 'United States' OR Country = 'USA' THEN 'US'
        WHEN Country = 'United Kingdom' OR Country = 'UK' THEN 'GB'
        WHEN Country = 'Israel' THEN 'IL'
        WHEN LEN(Country) = 2 THEN Country
        ELSE 'US'
    END as Country,
    CompanyWebsite as Website,
    ISNULL(CompanyEmail, CONCAT('info@', REPLACE(REPLACE(LOWER(SupplierName), ' ', ''), ',', ''), '.com')) as MainEmail,
    1 as IsActive,
    GETUTCDATE() as CreatedAt,
    GETUTCDATE() as UpdatedAt
FROM FoodXSuppliers
WHERE SupplierName IS NOT NULL 
AND SupplierName != ''
AND NOT EXISTS (SELECT 1 FROM Companies WHERE Name = FoodXSuppliers.SupplierName)
ORDER BY 
    CASE WHEN Country IS NOT NULL THEN 0 ELSE 1 END,
    SupplierName;

-- ============================================
-- 3. Create Sample Products for Suppliers
-- ============================================

-- Generate sample products for supplier companies
INSERT INTO Products (
    Id, Name, Description, Category, SKU, Price, Unit, 
    MinOrderQuantity, StockQuantity, Origin, 
    CompanyId, IsOrganic, IsAvailable, CreatedAt, UpdatedAt
)
SELECT TOP 500
    NEWID() as Id,
    CONCAT(
        CASE (ABS(CHECKSUM(NEWID())) % 5)
            WHEN 0 THEN 'Premium '
            WHEN 1 THEN 'Organic '
            WHEN 2 THEN 'Fresh '
            WHEN 3 THEN 'Quality '
            ELSE ''
        END,
        CASE (ABS(CHECKSUM(NEWID())) % 20)
            WHEN 0 THEN 'Tomatoes'
            WHEN 1 THEN 'Potatoes'
            WHEN 2 THEN 'Onions'
            WHEN 3 THEN 'Carrots'
            WHEN 4 THEN 'Apples'
            WHEN 5 THEN 'Oranges'
            WHEN 6 THEN 'Chicken'
            WHEN 7 THEN 'Beef'
            WHEN 8 THEN 'Salmon'
            WHEN 9 THEN 'Olive Oil'
            WHEN 10 THEN 'Pasta'
            WHEN 11 THEN 'Rice'
            WHEN 12 THEN 'Bread'
            WHEN 13 THEN 'Cheese'
            WHEN 14 THEN 'Milk'
            WHEN 15 THEN 'Eggs'
            WHEN 16 THEN 'Butter'
            WHEN 17 THEN 'Sugar'
            WHEN 18 THEN 'Flour'
            ELSE 'Mixed Products'
        END
    ) as Name,
    'High quality product from verified supplier' as Description,
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
    CONCAT('SKU', RIGHT('00000' + CAST(ROW_NUMBER() OVER (ORDER BY NEWID()) AS VARCHAR(5)), 5)) as SKU,
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
    END as MinOrderQuantity,
    ABS(CHECKSUM(NEWID())) % 10000 + 100 as StockQuantity,
    c.Country as Origin,
    c.Id as CompanyId,
    CASE WHEN ABS(CHECKSUM(NEWID())) % 5 = 0 THEN 1 ELSE 0 END as IsOrganic,
    1 as IsAvailable,
    GETUTCDATE() as CreatedAt,
    GETUTCDATE() as UpdatedAt
FROM Companies c
WHERE c.CompanyType = 'Supplier'
AND NOT EXISTS (SELECT 1 FROM Products WHERE CompanyId = c.Id)
ORDER BY NEWID();

-- ============================================
-- 4. Link to Test Users
-- ============================================

-- Create FoodXUsers entries for test accounts
INSERT INTO Users (Id, Email, FirstName, LastName, Role, CompanyId, IsActive, CreatedAt, UpdatedAt)
SELECT 
    (SELECT MAX(Id) FROM Users) + ROW_NUMBER() OVER (ORDER BY u.Email) as Id,
    u.Email,
    u.FirstName,
    u.LastName,
    r.Name as Role,
    CASE 
        WHEN r.Name = 'Buyer' THEN (SELECT TOP 1 Id FROM Companies WHERE CompanyType = 'Buyer' ORDER BY NEWID())
        WHEN r.Name = 'Supplier' THEN (SELECT TOP 1 Id FROM Companies WHERE CompanyType = 'Supplier' ORDER BY NEWID())
        ELSE NULL
    END as CompanyId,
    1 as IsActive,
    GETUTCDATE() as CreatedAt,
    GETUTCDATE() as UpdatedAt
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email LIKE '%@test.fdx.trading'
AND NOT EXISTS (SELECT 1 FROM Users WHERE Email = u.Email);

-- ============================================
-- 5. Verification
-- ============================================

-- Count records in each table
SELECT 'Companies' as TableName, COUNT(*) as RecordCount FROM Companies
UNION ALL
SELECT 'Products', COUNT(*) FROM Products
UNION ALL
SELECT 'Users', COUNT(*) FROM Users
UNION ALL
SELECT 'FoodXBuyers (source)', COUNT(*) FROM FoodXBuyers
UNION ALL
SELECT 'FoodXSuppliers (source)', COUNT(*) FROM FoodXSuppliers
ORDER BY TableName;

-- Show company distribution
SELECT CompanyType, COUNT(*) as Count 
FROM Companies 
GROUP BY CompanyType;

-- Show product categories
SELECT Category, COUNT(*) as Count 
FROM Products 
GROUP BY Category
ORDER BY Count DESC;

-- Show sample companies
SELECT TOP 10 
    c.Id,
    c.Name,
    c.CompanyType,
    c.Country,
    (SELECT COUNT(*) FROM Products WHERE CompanyId = c.Id) as ProductCount
FROM Companies c
ORDER BY c.CreatedAt DESC;
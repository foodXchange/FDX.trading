-- Create SupplierProducts table for individual product records
-- This enables better product-based matching for buyer requests

-- Drop table if exists (for clean setup)
IF OBJECT_ID('dbo.SupplierProducts', 'U') IS NOT NULL
    DROP TABLE dbo.SupplierProducts;
GO

-- Create the SupplierProducts table
CREATE TABLE dbo.SupplierProducts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SupplierId INT NOT NULL,
    SupplierName NVARCHAR(255),
    ProductName NVARCHAR(500),
    Category NVARCHAR(200),
    Brand NVARCHAR(200),
    Country NVARCHAR(100),
    IsKosher BIT DEFAULT 0,
    IsHalal BIT DEFAULT 0,
    IsOrganic BIT DEFAULT 0,
    IsGlutenFree BIT DEFAULT 0,
    SearchText NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
);
GO

-- Create indexes for performance
CREATE INDEX IX_SupplierProducts_SupplierId ON SupplierProducts(SupplierId);
CREATE INDEX IX_SupplierProducts_SupplierName ON SupplierProducts(SupplierName);
CREATE INDEX IX_SupplierProducts_ProductName ON SupplierProducts(ProductName);
CREATE INDEX IX_SupplierProducts_Category ON SupplierProducts(Category);
CREATE INDEX IX_SupplierProducts_Country ON SupplierProducts(Country);
CREATE INDEX IX_SupplierProducts_Kosher ON SupplierProducts(IsKosher) WHERE IsKosher = 1;
CREATE INDEX IX_SupplierProducts_Halal ON SupplierProducts(IsHalal) WHERE IsHalal = 1;
GO

-- Full-text search can be added later if needed
-- For now we'll use LIKE queries for searching

PRINT 'SupplierProducts table created successfully';
PRINT 'Indexes created for optimal search performance';

-- Check the table
SELECT 
    'SupplierProducts' as TableName,
    COUNT(*) as RecordCount
FROM SupplierProducts;

-- Show table structure
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SupplierProducts'
ORDER BY ORDINAL_POSITION;
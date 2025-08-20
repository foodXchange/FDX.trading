-- Check if Products table exists and create if not
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Products' AND xtype='U')
BEGIN
    CREATE TABLE Products (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Category NVARCHAR(50),
        Description NVARCHAR(1000),
        Price DECIMAL(18,2) NOT NULL,
        Unit NVARCHAR(20) NOT NULL,
        MinOrderQuantity INT DEFAULT 1,
        StockQuantity INT DEFAULT 0,
        SKU NVARCHAR(50),
        Origin NVARCHAR(100),
        IsOrganic BIT DEFAULT 0,
        IsAvailable BIT DEFAULT 1,
        ImageUrl NVARCHAR(500),
        SupplierId INT NULL,
        CompanyId INT NULL,
        CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
    );
    
    PRINT 'Products table created successfully';
END
ELSE
BEGIN
    PRINT 'Products table already exists';
END

-- Check and create indexes if they don't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_Category')
    CREATE INDEX IX_Products_Category ON Products(Category);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_SupplierId')
    CREATE INDEX IX_Products_SupplierId ON Products(SupplierId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_CompanyId')
    CREATE INDEX IX_Products_CompanyId ON Products(CompanyId);

-- Check if table is empty and add sample data
IF NOT EXISTS (SELECT 1 FROM Products)
BEGIN
    INSERT INTO Products (Name, Category, Description, Price, Unit, MinOrderQuantity, StockQuantity, SKU, Origin, IsOrganic, IsAvailable)
    VALUES 
        ('Organic Red Apples', 'Fruits', 'Fresh organic red apples from local farms', 3.99, 'kg', 5, 100, 'FRT-APL-001', 'Washington, USA', 1, 1),
        ('Fresh Tomatoes', 'Vegetables', 'Vine-ripened tomatoes, perfect for salads', 2.49, 'kg', 2, 50, 'VEG-TOM-001', 'California, USA', 0, 1),
        ('Whole Milk', 'Dairy', 'Fresh whole milk from grass-fed cows', 4.99, 'gallon', 1, 20, 'DRY-MLK-001', 'Wisconsin, USA', 1, 1),
        ('Chicken Breast', 'Meat', 'Boneless, skinless chicken breast', 8.99, 'kg', 3, 30, 'MT-CHK-001', 'Texas, USA', 0, 1),
        ('Atlantic Salmon', 'Seafood', 'Fresh Atlantic salmon fillets', 12.99, 'kg', 2, 15, 'SEA-SAL-001', 'Norway', 0, 1),
        ('Brown Rice', 'Grains', 'Organic whole grain brown rice', 2.99, 'kg', 10, 200, 'GRN-RCE-001', 'Thailand', 1, 1);
    
    PRINT 'Sample products added';
END

-- Display table info
SELECT COUNT(*) AS 'Total Products' FROM Products;
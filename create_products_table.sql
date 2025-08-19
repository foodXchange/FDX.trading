-- Create Products table
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
    SupplierId INT,
    CompanyId INT,
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    
    -- Foreign keys
    CONSTRAINT FK_Products_Suppliers FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id),
    CONSTRAINT FK_Products_Companies FOREIGN KEY (CompanyId) REFERENCES Companies(Id)
);

-- Create index on Category for better search performance
CREATE INDEX IX_Products_Category ON Products(Category);

-- Create index on SupplierId for better join performance
CREATE INDEX IX_Products_SupplierId ON Products(SupplierId);

-- Create index on CompanyId for better join performance
CREATE INDEX IX_Products_CompanyId ON Products(CompanyId);

-- Insert some sample products
INSERT INTO Products (Name, Category, Description, Price, Unit, MinOrderQuantity, StockQuantity, SKU, Origin, IsOrganic, IsAvailable, CompanyId)
VALUES 
    ('Organic Red Apples', 'Fruits', 'Fresh organic red apples from local farms', 3.99, 'kg', 5, 100, 'FRT-APL-001', 'Washington, USA', 1, 1, 1),
    ('Fresh Tomatoes', 'Vegetables', 'Vine-ripened tomatoes, perfect for salads', 2.49, 'kg', 2, 50, 'VEG-TOM-001', 'California, USA', 0, 1, 1),
    ('Whole Milk', 'Dairy', 'Fresh whole milk from grass-fed cows', 4.99, 'gallon', 1, 20, 'DRY-MLK-001', 'Wisconsin, USA', 1, 1, 2),
    ('Chicken Breast', 'Meat', 'Boneless, skinless chicken breast', 8.99, 'kg', 3, 30, 'MT-CHK-001', 'Texas, USA', 0, 1, 2),
    ('Atlantic Salmon', 'Seafood', 'Fresh Atlantic salmon fillets', 12.99, 'kg', 2, 15, 'SEA-SAL-001', 'Norway', 0, 1, 3),
    ('Brown Rice', 'Grains', 'Organic whole grain brown rice', 2.99, 'kg', 10, 200, 'GRN-RCE-001', 'Thailand', 1, 1, 3);

PRINT 'Products table created successfully';
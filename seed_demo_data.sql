-- Seed Demo Data for FoodX Trading Platform

-- Check and create Companies table if needed
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Companies')
BEGIN
    CREATE TABLE Companies (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        CompanyType NVARCHAR(50) NOT NULL,
        BuyerCategory NVARCHAR(50),
        Country NVARCHAR(100),
        Address NVARCHAR(500),
        WarehouseAddress NVARCHAR(500),
        MainEmail NVARCHAR(256),
        MainPhone NVARCHAR(50),
        VatNumber NVARCHAR(50),
        Website NVARCHAR(256),
        Description NVARCHAR(MAX),
        LogoUrl NVARCHAR(500),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2
    );
    PRINT 'Companies table created';
END

-- Check and create Products table if needed  
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products')
BEGIN
    CREATE TABLE Products (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Category NVARCHAR(100),
        Description NVARCHAR(MAX),
        Unit NVARCHAR(50),
        MinimumOrder DECIMAL(18,2),
        Price DECIMAL(18,2),
        Currency NVARCHAR(10),
        SupplierId INT,
        SKU NVARCHAR(100),
        ImageUrl NVARCHAR(500),
        IsActive BIT NOT NULL DEFAULT 1,
        StockQuantity INT,
        Origin NVARCHAR(100),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2,
        FOREIGN KEY (SupplierId) REFERENCES Companies(Id)
    );
    PRINT 'Products table created';
END

-- Clear existing demo data
DELETE FROM Products WHERE Name LIKE '%Demo%' OR Description LIKE '%demo%';
DELETE FROM Companies WHERE Name LIKE '%Demo%' OR Description LIKE '%demo%';

-- Insert Supplier Companies
INSERT INTO Companies (Name, CompanyType, Country, Address, MainEmail, MainPhone, VatNumber, Website, Description, IsActive)
VALUES 
    ('Green Valley Farms', 'Supplier', 'United States', '123 Farm Road, California, USA', 'contact@greenvalley.com', '+1-555-0100', 'US123456789', 'www.greenvalleyfarms.com', 'Premium organic produce supplier specializing in fresh vegetables and fruits', 1),
    ('Mediterranean Delights Co.', 'Supplier', 'Italy', '456 Via Roma, Milan, Italy', 'info@meddelights.it', '+39-02-1234567', 'IT987654321', 'www.mediterraneandelights.it', 'Authentic Italian food products including olive oil, pasta, and cheeses', 1),
    ('Asian Spice Trading', 'Supplier', 'Thailand', '789 Sukhumvit Road, Bangkok, Thailand', 'sales@asianspice.th', '+66-2-9876543', 'TH456789123', 'www.asianspicetrading.com', 'Leading exporter of Asian spices, herbs, and specialty ingredients', 1),
    ('Nordic Seafood AB', 'Supplier', 'Norway', '321 Harbor Street, Bergen, Norway', 'export@nordicseafood.no', '+47-55-123456', 'NO876543210', 'www.nordicseafood.no', 'Fresh and frozen seafood from Norwegian waters', 1),
    ('Tropical Fruits Export', 'Supplier', 'Brazil', '654 Av. Paulista, São Paulo, Brazil', 'contact@tropicalfruits.br', '+55-11-98765432', 'BR345678901', 'www.tropicalfruitsexport.com.br', 'Exotic tropical fruits direct from Brazilian farms', 1);

-- Insert Buyer Companies  
INSERT INTO Companies (Name, CompanyType, BuyerCategory, Country, Address, WarehouseAddress, MainEmail, MainPhone, VatNumber, Website, Description, IsActive)
VALUES 
    ('Global Restaurant Group', 'Buyer', 'Restaurant Chain', 'United Kingdom', '100 Oxford Street, London, UK', '50 Industrial Park, London, UK', 'purchasing@globalrestaurants.uk', '+44-20-7123456', 'GB123456789', 'www.globalrestaurantgroup.com', 'International restaurant chain with 500+ locations', 1),
    ('City Market Wholesale', 'Buyer', 'Distributor', 'Germany', '200 Hauptstraße, Berlin, Germany', '75 Logistics Center, Berlin, Germany', 'buyers@citymarket.de', '+49-30-9876543', 'DE987654321', 'www.citymarketwholesale.de', 'Major food distributor serving retail stores across Europe', 1),
    ('Hotel Paradise Chain', 'Buyer', 'Hotel', 'France', '300 Champs-Élysées, Paris, France', '25 Storage Facility, Paris, France', 'procurement@hotelparadise.fr', '+33-1-87654321', 'FR456789123', 'www.hotelparadise.com', 'Luxury hotel chain with properties worldwide', 1),
    ('Quick Serve Foods Inc.', 'Buyer', 'Fast Food Chain', 'Canada', '400 Bay Street, Toronto, Canada', '100 Distribution Center, Toronto, Canada', 'supply@quickserve.ca', '+1-416-5551234', 'CA876543210', 'www.quickservefoods.ca', 'Fast food franchise with 1000+ locations', 1),
    ('Gourmet Catering Services', 'Buyer', 'Catering', 'Australia', '500 George Street, Sydney, Australia', '30 Food Prep Center, Sydney, Australia', 'orders@gourmetcatering.au', '+61-2-98765432', 'AU345678901', 'www.gourmetcateringservices.com.au', 'Premium catering for corporate events and weddings', 1);

-- Get Supplier IDs for Products
DECLARE @GreenValleyId INT = (SELECT Id FROM Companies WHERE Name = 'Green Valley Farms');
DECLARE @MedDelightsId INT = (SELECT Id FROM Companies WHERE Name = 'Mediterranean Delights Co.');
DECLARE @AsianSpiceId INT = (SELECT Id FROM Companies WHERE Name = 'Asian Spice Trading');
DECLARE @NordicSeafoodId INT = (SELECT Id FROM Companies WHERE Name = 'Nordic Seafood AB');
DECLARE @TropicalFruitsId INT = (SELECT Id FROM Companies WHERE Name = 'Tropical Fruits Export');

-- Insert Products
INSERT INTO Products (Name, Category, Description, Unit, MinimumOrder, Price, Currency, SupplierId, SKU, Origin, StockQuantity, IsActive)
VALUES 
    -- Green Valley Farms Products
    ('Organic Tomatoes', 'Vegetables', 'Fresh organic roma tomatoes', 'kg', 10, 3.50, 'USD', @GreenValleyId, 'GVF-TOM-001', 'California, USA', 500, 1),
    ('Organic Lettuce', 'Vegetables', 'Crisp organic iceberg lettuce', 'head', 20, 2.00, 'USD', @GreenValleyId, 'GVF-LET-001', 'California, USA', 300, 1),
    ('Organic Carrots', 'Vegetables', 'Sweet organic carrots', 'kg', 15, 2.50, 'USD', @GreenValleyId, 'GVF-CAR-001', 'California, USA', 400, 1),
    
    -- Mediterranean Delights Products
    ('Extra Virgin Olive Oil', 'Oils', 'Premium cold-pressed olive oil', 'liter', 12, 15.00, 'EUR', @MedDelightsId, 'MD-OIL-001', 'Tuscany, Italy', 200, 1),
    ('Parmigiano Reggiano', 'Dairy', 'Aged 24 months parmesan cheese', 'kg', 5, 35.00, 'EUR', @MedDelightsId, 'MD-CHE-001', 'Parma, Italy', 100, 1),
    ('Artisan Pasta', 'Pasta', 'Handmade traditional pasta', 'kg', 10, 8.00, 'EUR', @MedDelightsId, 'MD-PAS-001', 'Rome, Italy', 300, 1),
    
    -- Asian Spice Trading Products
    ('Thai Jasmine Rice', 'Grains', 'Premium fragrant jasmine rice', 'kg', 25, 2.20, 'USD', @AsianSpiceId, 'AST-RIC-001', 'Thailand', 1000, 1),
    ('Lemongrass', 'Herbs', 'Fresh lemongrass stalks', 'kg', 5, 12.00, 'USD', @AsianSpiceId, 'AST-LEM-001', 'Thailand', 150, 1),
    ('Thai Chili Peppers', 'Spices', 'Hot Thai bird eye chilies', 'kg', 3, 18.00, 'USD', @AsianSpiceId, 'AST-CHI-001', 'Thailand', 100, 1),
    
    -- Nordic Seafood Products
    ('Atlantic Salmon', 'Seafood', 'Fresh Atlantic salmon fillets', 'kg', 10, 25.00, 'EUR', @NordicSeafoodId, 'NS-SAL-001', 'Norway', 200, 1),
    ('King Crab', 'Seafood', 'Premium king crab legs', 'kg', 5, 85.00, 'EUR', @NordicSeafoodId, 'NS-CRA-001', 'Norway', 50, 1),
    ('Cod Fillets', 'Seafood', 'Fresh Norwegian cod fillets', 'kg', 15, 18.00, 'EUR', @NordicSeafoodId, 'NS-COD-001', 'Norway', 150, 1),
    
    -- Tropical Fruits Export Products
    ('Mangoes', 'Fruits', 'Sweet Ataulfo mangoes', 'kg', 20, 4.50, 'USD', @TropicalFruitsId, 'TFE-MAN-001', 'Brazil', 400, 1),
    ('Passion Fruit', 'Fruits', 'Fresh passion fruit', 'kg', 10, 8.00, 'USD', @TropicalFruitsId, 'TFE-PAS-001', 'Brazil', 200, 1),
    ('Dragon Fruit', 'Fruits', 'Exotic dragon fruit', 'kg', 15, 12.00, 'USD', @TropicalFruitsId, 'TFE-DRA-001', 'Brazil', 150, 1);

-- Report on inserted data
SELECT 'Demo Data Summary' AS Report;
SELECT 'Total Suppliers' AS Metric, COUNT(*) AS Count FROM Companies WHERE CompanyType = 'Supplier';
SELECT 'Total Buyers' AS Metric, COUNT(*) AS Count FROM Companies WHERE CompanyType = 'Buyer';
SELECT 'Total Products' AS Metric, COUNT(*) AS Count FROM Products WHERE IsActive = 1;

PRINT 'Demo data seeded successfully!';
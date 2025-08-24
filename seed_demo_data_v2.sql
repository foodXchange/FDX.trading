-- Seed Demo Data for FoodX Trading Platform (Simplified)

-- Clear any existing demo data
DELETE FROM Products WHERE Name LIKE '%Demo%' OR Name LIKE '%Organic%' OR Name LIKE '%Premium%';
DELETE FROM Companies WHERE Name LIKE '%Demo%' OR Name LIKE '%Valley%' OR Name LIKE '%Mediterranean%';

-- Insert Supplier Companies
INSERT INTO Companies (Name, CompanyType, Country, Address, MainEmail, MainPhone, VatNumber, Website, Description, IsActive, CreatedAt)
VALUES 
    ('Green Valley Farms', 'Supplier', 'United States', '123 Farm Road, California, USA', 'contact@greenvalley.com', '+1-555-0100', 'US123456789', 'www.greenvalleyfarms.com', 'Premium organic produce supplier specializing in fresh vegetables and fruits', 1, GETUTCDATE()),
    ('Mediterranean Delights Co.', 'Supplier', 'Italy', '456 Via Roma, Milan, Italy', 'info@meddelights.it', '+39-02-1234567', 'IT987654321', 'www.mediterraneandelights.it', 'Authentic Italian food products including olive oil, pasta, and cheeses', 1, GETUTCDATE()),
    ('Asian Spice Trading', 'Supplier', 'Thailand', '789 Sukhumvit Road, Bangkok, Thailand', 'sales@asianspice.th', '+66-2-9876543', 'TH456789123', 'www.asianspicetrading.com', 'Leading exporter of Asian spices, herbs, and specialty ingredients', 1, GETUTCDATE()),
    ('Nordic Seafood AB', 'Supplier', 'Norway', '321 Harbor Street, Bergen, Norway', 'export@nordicseafood.no', '+47-55-123456', 'NO876543210', 'www.nordicseafood.no', 'Fresh and frozen seafood from Norwegian waters', 1, GETUTCDATE()),
    ('Tropical Fruits Export', 'Supplier', 'Brazil', '654 Av. Paulista, São Paulo, Brazil', 'contact@tropicalfruits.br', '+55-11-98765432', 'BR345678901', 'www.tropicalfruitsexport.com.br', 'Exotic tropical fruits direct from Brazilian farms', 1, GETUTCDATE());

-- Insert Buyer Companies  
INSERT INTO Companies (Name, CompanyType, BuyerCategory, Country, Address, WarehouseAddress, MainEmail, MainPhone, VatNumber, Website, Description, IsActive, CreatedAt)
VALUES 
    ('Global Restaurant Group', 'Buyer', 'Restaurant Chain', 'United Kingdom', '100 Oxford Street, London, UK', '50 Industrial Park, London, UK', 'purchasing@globalrestaurants.uk', '+44-20-7123456', 'GB123456789', 'www.globalrestaurantgroup.com', 'International restaurant chain with 500+ locations', 1, GETUTCDATE()),
    ('City Market Wholesale', 'Buyer', 'Distributor', 'Germany', '200 Hauptstraße, Berlin, Germany', '75 Logistics Center, Berlin, Germany', 'buyers@citymarket.de', '+49-30-9876543', 'DE987654321', 'www.citymarketwholesale.de', 'Major food distributor serving retail stores across Europe', 1, GETUTCDATE()),
    ('Hotel Paradise Chain', 'Buyer', 'Hotel', 'France', '300 Champs-Élysées, Paris, France', '25 Storage Facility, Paris, France', 'procurement@hotelparadise.fr', '+33-1-87654321', 'FR456789123', 'www.hotelparadise.com', 'Luxury hotel chain with properties worldwide', 1, GETUTCDATE()),
    ('Quick Serve Foods Inc.', 'Buyer', 'Fast Food Chain', 'Canada', '400 Bay Street, Toronto, Canada', '100 Distribution Center, Toronto, Canada', 'supply@quickserve.ca', '+1-416-5551234', 'CA876543210', 'www.quickservefoods.ca', 'Fast food franchise with 1000+ locations', 1, GETUTCDATE()),
    ('Gourmet Catering Services', 'Buyer', 'Catering', 'Australia', '500 George Street, Sydney, Australia', '30 Food Prep Center, Sydney, Australia', 'orders@gourmetcatering.au', '+61-2-98765432', 'AU345678901', 'www.gourmetcateringservices.com.au', 'Premium catering for corporate events and weddings', 1, GETUTCDATE());

-- Get Supplier IDs for Products
DECLARE @GreenValleyId INT = (SELECT TOP 1 Id FROM Companies WHERE Name = 'Green Valley Farms');
DECLARE @MedDelightsId INT = (SELECT TOP 1 Id FROM Companies WHERE Name = 'Mediterranean Delights Co.');
DECLARE @AsianSpiceId INT = (SELECT TOP 1 Id FROM Companies WHERE Name = 'Asian Spice Trading');
DECLARE @NordicSeafoodId INT = (SELECT TOP 1 Id FROM Companies WHERE Name = 'Nordic Seafood AB');
DECLARE @TropicalFruitsId INT = (SELECT TOP 1 Id FROM Companies WHERE Name = 'Tropical Fruits Export');

-- Insert Products (using only existing columns)
INSERT INTO Products (Name, Category, Description, Unit, Price, SupplierId, SKU, Origin, StockQuantity, IsActive, CreatedAt)
VALUES 
    -- Green Valley Farms Products
    ('Organic Tomatoes', 'Vegetables', 'Fresh organic roma tomatoes', 'kg', 3.50, @GreenValleyId, 'GVF-TOM-001', 'California, USA', 500, 1, GETUTCDATE()),
    ('Organic Lettuce', 'Vegetables', 'Crisp organic iceberg lettuce', 'head', 2.00, @GreenValleyId, 'GVF-LET-001', 'California, USA', 300, 1, GETUTCDATE()),
    ('Organic Carrots', 'Vegetables', 'Sweet organic carrots', 'kg', 2.50, @GreenValleyId, 'GVF-CAR-001', 'California, USA', 400, 1, GETUTCDATE()),
    
    -- Mediterranean Delights Products
    ('Extra Virgin Olive Oil', 'Oils', 'Premium cold-pressed olive oil', 'liter', 15.00, @MedDelightsId, 'MD-OIL-001', 'Tuscany, Italy', 200, 1, GETUTCDATE()),
    ('Parmigiano Reggiano', 'Dairy', 'Aged 24 months parmesan cheese', 'kg', 35.00, @MedDelightsId, 'MD-CHE-001', 'Parma, Italy', 100, 1, GETUTCDATE()),
    ('Artisan Pasta', 'Pasta', 'Handmade traditional pasta', 'kg', 8.00, @MedDelightsId, 'MD-PAS-001', 'Rome, Italy', 300, 1, GETUTCDATE()),
    
    -- Asian Spice Trading Products
    ('Thai Jasmine Rice', 'Grains', 'Premium fragrant jasmine rice', 'kg', 2.20, @AsianSpiceId, 'AST-RIC-001', 'Thailand', 1000, 1, GETUTCDATE()),
    ('Lemongrass', 'Herbs', 'Fresh lemongrass stalks', 'kg', 12.00, @AsianSpiceId, 'AST-LEM-001', 'Thailand', 150, 1, GETUTCDATE()),
    ('Thai Chili Peppers', 'Spices', 'Hot Thai bird eye chilies', 'kg', 18.00, @AsianSpiceId, 'AST-CHI-001', 'Thailand', 100, 1, GETUTCDATE()),
    
    -- Nordic Seafood Products
    ('Atlantic Salmon', 'Seafood', 'Fresh Atlantic salmon fillets', 'kg', 25.00, @NordicSeafoodId, 'NS-SAL-001', 'Norway', 200, 1, GETUTCDATE()),
    ('King Crab', 'Seafood', 'Premium king crab legs', 'kg', 85.00, @NordicSeafoodId, 'NS-CRA-001', 'Norway', 50, 1, GETUTCDATE()),
    ('Cod Fillets', 'Seafood', 'Fresh Norwegian cod fillets', 'kg', 18.00, @NordicSeafoodId, 'NS-COD-001', 'Norway', 150, 1, GETUTCDATE()),
    
    -- Tropical Fruits Export Products
    ('Mangoes', 'Fruits', 'Sweet Ataulfo mangoes', 'kg', 4.50, @TropicalFruitsId, 'TFE-MAN-001', 'Brazil', 400, 1, GETUTCDATE()),
    ('Passion Fruit', 'Fruits', 'Fresh passion fruit', 'kg', 8.00, @TropicalFruitsId, 'TFE-PAS-001', 'Brazil', 200, 1, GETUTCDATE()),
    ('Dragon Fruit', 'Fruits', 'Exotic dragon fruit', 'kg', 12.00, @TropicalFruitsId, 'TFE-DRA-001', 'Brazil', 150, 1, GETUTCDATE());

-- Report on inserted data
PRINT '=== Demo Data Summary ===';
SELECT 'Total Suppliers' AS Metric, COUNT(*) AS Count FROM Companies WHERE CompanyType = 'Supplier';
SELECT 'Total Buyers' AS Metric, COUNT(*) AS Count FROM Companies WHERE CompanyType = 'Buyer';
SELECT 'Total Products' AS Metric, COUNT(*) AS Count FROM Products WHERE IsActive = 1;

PRINT 'Demo data seeded successfully!';
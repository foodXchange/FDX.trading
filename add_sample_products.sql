-- Add Sample Products to Database
-- Simple script to populate Products table

-- Get the next available ID
DECLARE @StartId INT = ISNULL((SELECT MAX(Id) FROM Products), 0) + 1;

-- Insert sample fruits
INSERT INTO Products (Id, Name, Category, Description, Price, SKU, Unit, MinOrderQuantity, StockQuantity, Origin, IsOrganic, IsActive, CreatedAt, UpdatedAt)
VALUES
(@StartId, 'Premium Apples', 'Fruits', 'Fresh red apples from premium orchards', 3.99, 'SKU-APL001', 'KG', 10, 500, 'US', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+1, 'Organic Bananas', 'Fruits', 'Certified organic bananas', 2.49, 'SKU-BAN001', 'KG', 25, 1000, 'EC', 1, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+2, 'Fresh Oranges', 'Fruits', 'Juicy Valencia oranges', 2.99, 'SKU-ORG001', 'KG', 20, 750, 'ES', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+3, 'Strawberries', 'Fruits', 'Sweet fresh strawberries', 5.99, 'SKU-STR001', 'KG', 5, 200, 'US', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+4, 'Organic Blueberries', 'Fruits', 'Premium organic blueberries', 8.99, 'SKU-BLU001', 'KG', 5, 150, 'CA', 1, 1, GETUTCDATE(), GETUTCDATE());

-- Insert sample vegetables
INSERT INTO Products (Id, Name, Category, Description, Price, SKU, Unit, MinOrderQuantity, StockQuantity, Origin, IsOrganic, IsActive, CreatedAt, UpdatedAt)
VALUES
(@StartId+5, 'Fresh Tomatoes', 'Vegetables', 'Vine-ripened tomatoes', 2.49, 'SKU-TOM001', 'KG', 15, 800, 'IL', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+6, 'Organic Carrots', 'Vegetables', 'Organic fresh carrots', 1.99, 'SKU-CAR001', 'KG', 20, 1200, 'NL', 1, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+7, 'Red Potatoes', 'Vegetables', 'Premium red potatoes', 1.49, 'SKU-POT001', 'KG', 50, 2000, 'US', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+8, 'Fresh Onions', 'Vegetables', 'Yellow cooking onions', 1.29, 'SKU-ONI001', 'KG', 25, 1500, 'ES', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+9, 'Organic Lettuce', 'Vegetables', 'Crisp organic lettuce', 2.99, 'SKU-LET001', 'UNIT', 24, 500, 'US', 1, 1, GETUTCDATE(), GETUTCDATE());

-- Insert sample meat products
INSERT INTO Products (Id, Name, Category, Description, Price, SKU, Unit, MinOrderQuantity, StockQuantity, Origin, IsOrganic, IsActive, CreatedAt, UpdatedAt)
VALUES
(@StartId+10, 'Chicken Breast', 'Meat', 'Fresh boneless chicken breast', 7.99, 'SKU-CHK001', 'KG', 10, 300, 'US', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+11, 'Beef Ribeye', 'Meat', 'Premium ribeye steak', 24.99, 'SKU-BEF001', 'KG', 5, 100, 'US', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+12, 'Ground Turkey', 'Meat', 'Lean ground turkey', 5.99, 'SKU-TRK001', 'KG', 10, 250, 'US', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+13, 'Pork Chops', 'Meat', 'Center-cut pork chops', 8.99, 'SKU-PRK001', 'KG', 10, 200, 'DE', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+14, 'Organic Lamb', 'Meat', 'Organic lamb shoulder', 18.99, 'SKU-LMB001', 'KG', 5, 50, 'NZ', 1, 1, GETUTCDATE(), GETUTCDATE());

-- Insert sample seafood
INSERT INTO Products (Id, Name, Category, Description, Price, SKU, Unit, MinOrderQuantity, StockQuantity, Origin, IsOrganic, IsActive, CreatedAt, UpdatedAt)
VALUES
(@StartId+15, 'Atlantic Salmon', 'Seafood', 'Fresh Atlantic salmon fillet', 14.99, 'SKU-SAL001', 'KG', 5, 150, 'NO', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+16, 'Wild Tuna', 'Seafood', 'Wild-caught tuna steaks', 19.99, 'SKU-TUN001', 'KG', 5, 80, 'JP', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+17, 'Fresh Shrimp', 'Seafood', 'Large fresh shrimp', 16.99, 'SKU-SHR001', 'KG', 10, 200, 'TH', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+18, 'Sea Bass', 'Seafood', 'Mediterranean sea bass', 12.99, 'SKU-BAS001', 'KG', 5, 100, 'GR', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+19, 'Lobster Tails', 'Seafood', 'Premium lobster tails', 34.99, 'SKU-LOB001', 'KG', 2, 30, 'CA', 0, 1, GETUTCDATE(), GETUTCDATE());

-- Insert sample dairy products
INSERT INTO Products (Id, Name, Category, Description, Price, SKU, Unit, MinOrderQuantity, StockQuantity, Origin, IsOrganic, IsActive, CreatedAt, UpdatedAt)
VALUES
(@StartId+20, 'Whole Milk', 'Dairy', 'Fresh whole milk', 3.49, 'SKU-MLK001', 'LB', 12, 500, 'US', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+21, 'Organic Yogurt', 'Dairy', 'Organic Greek yogurt', 5.99, 'SKU-YOG001', 'CASE', 24, 300, 'GR', 1, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+22, 'Cheddar Cheese', 'Dairy', 'Aged cheddar cheese', 8.99, 'SKU-CHE001', 'KG', 5, 200, 'GB', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+23, 'Fresh Butter', 'Dairy', 'Premium fresh butter', 4.99, 'SKU-BUT001', 'KG', 10, 400, 'FR', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+24, 'Organic Eggs', 'Dairy', 'Free-range organic eggs', 6.99, 'SKU-EGG001', 'CASE', 30, 1000, 'US', 1, 1, GETUTCDATE(), GETUTCDATE());

-- Insert sample grains
INSERT INTO Products (Id, Name, Category, Description, Price, SKU, Unit, MinOrderQuantity, StockQuantity, Origin, IsOrganic, IsActive, CreatedAt, UpdatedAt)
VALUES
(@StartId+25, 'Basmati Rice', 'Grains', 'Premium basmati rice', 3.99, 'SKU-RIC001', 'KG', 25, 1000, 'IN', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+26, 'Whole Wheat Flour', 'Grains', 'Stone-ground whole wheat flour', 2.49, 'SKU-FLR001', 'KG', 20, 800, 'US', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+27, 'Organic Quinoa', 'Grains', 'Organic white quinoa', 7.99, 'SKU-QUI001', 'KG', 10, 300, 'PE', 1, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+28, 'Pasta Penne', 'Grains', 'Italian penne pasta', 2.99, 'SKU-PAS001', 'KG', 20, 600, 'IT', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+29, 'Rolled Oats', 'Grains', 'Premium rolled oats', 3.49, 'SKU-OAT001', 'KG', 15, 500, 'CA', 0, 1, GETUTCDATE(), GETUTCDATE());

-- Insert sample oils
INSERT INTO Products (Id, Name, Category, Description, Price, SKU, Unit, MinOrderQuantity, StockQuantity, Origin, IsOrganic, IsActive, CreatedAt, UpdatedAt)
VALUES
(@StartId+30, 'Extra Virgin Olive Oil', 'Oils', 'Premium extra virgin olive oil', 12.99, 'SKU-OLV001', 'LB', 6, 200, 'IT', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+31, 'Organic Coconut Oil', 'Oils', 'Cold-pressed organic coconut oil', 9.99, 'SKU-COC001', 'LB', 12, 150, 'PH', 1, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+32, 'Sunflower Oil', 'Oils', 'Pure sunflower oil', 4.99, 'SKU-SUN001', 'LB', 12, 300, 'UA', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+33, 'Avocado Oil', 'Oils', 'Premium avocado oil', 14.99, 'SKU-AVO001', 'LB', 6, 100, 'MX', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+34, 'Sesame Oil', 'Oils', 'Pure sesame oil', 8.99, 'SKU-SES001', 'LB', 12, 150, 'JP', 0, 1, GETUTCDATE(), GETUTCDATE());

-- Insert sample beverages
INSERT INTO Products (Id, Name, Category, Description, Price, SKU, Unit, MinOrderQuantity, StockQuantity, Origin, IsOrganic, IsActive, CreatedAt, UpdatedAt)
VALUES
(@StartId+35, 'Orange Juice', 'Beverages', '100% pure orange juice', 4.99, 'SKU-OJU001', 'CASE', 12, 400, 'US', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+36, 'Organic Green Tea', 'Beverages', 'Premium organic green tea', 6.99, 'SKU-TEA001', 'CASE', 24, 300, 'JP', 1, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+37, 'Colombian Coffee', 'Beverages', 'Premium Colombian coffee beans', 11.99, 'SKU-COF001', 'KG', 5, 200, 'CO', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+38, 'Sparkling Water', 'Beverages', 'Natural sparkling water', 2.99, 'SKU-WAT001', 'CASE', 24, 800, 'FR', 0, 1, GETUTCDATE(), GETUTCDATE()),
(@StartId+39, 'Organic Kombucha', 'Beverages', 'Organic fermented kombucha', 5.99, 'SKU-KOM001', 'CASE', 12, 150, 'US', 1, 1, GETUTCDATE(), GETUTCDATE());

-- Show summary
SELECT COUNT(*) as TotalProducts, COUNT(DISTINCT Category) as Categories FROM Products;
SELECT Category, COUNT(*) as Count FROM Products GROUP BY Category ORDER BY Category;
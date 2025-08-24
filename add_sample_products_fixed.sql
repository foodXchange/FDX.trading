-- Add Sample Products to Database (Fixed for Identity Column)
-- Products table has auto-increment ID, so we don't specify it

-- Insert sample fruits
INSERT INTO Products (Name, Category, Description, Price, SKU, Unit, MinOrderQuantity, StockQuantity, Origin, IsOrganic, IsActive, CreatedAt, UpdatedAt)
VALUES
('Premium Apples', 'Fruits', 'Fresh red apples from premium orchards', 3.99, 'SKU-APL001', 'KG', 10, 500, 'US', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Organic Bananas', 'Fruits', 'Certified organic bananas', 2.49, 'SKU-BAN001', 'KG', 25, 1000, 'EC', 1, 1, GETUTCDATE(), GETUTCDATE()),
('Fresh Oranges', 'Fruits', 'Juicy Valencia oranges', 2.99, 'SKU-ORG001', 'KG', 20, 750, 'ES', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Strawberries', 'Fruits', 'Sweet fresh strawberries', 5.99, 'SKU-STR001', 'KG', 5, 200, 'US', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Organic Blueberries', 'Fruits', 'Premium organic blueberries', 8.99, 'SKU-BLU001', 'KG', 5, 150, 'CA', 1, 1, GETUTCDATE(), GETUTCDATE());

-- Insert sample vegetables
INSERT INTO Products (Name, Category, Description, Price, SKU, Unit, MinOrderQuantity, StockQuantity, Origin, IsOrganic, IsActive, CreatedAt, UpdatedAt)
VALUES
('Garden Tomatoes', 'Vegetables', 'Vine-ripened tomatoes', 2.49, 'SKU-TOM002', 'KG', 15, 800, 'IL', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Organic Carrots', 'Vegetables', 'Organic fresh carrots', 1.99, 'SKU-CAR001', 'KG', 20, 1200, 'NL', 1, 1, GETUTCDATE(), GETUTCDATE()),
('Red Potatoes', 'Vegetables', 'Premium red potatoes', 1.49, 'SKU-POT001', 'KG', 50, 2000, 'US', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Fresh Onions', 'Vegetables', 'Yellow cooking onions', 1.29, 'SKU-ONI001', 'KG', 25, 1500, 'ES', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Organic Lettuce', 'Vegetables', 'Crisp organic lettuce', 2.99, 'SKU-LET001', 'UNIT', 24, 500, 'US', 1, 1, GETUTCDATE(), GETUTCDATE());

-- Insert sample meat products
INSERT INTO Products (Name, Category, Description, Price, SKU, Unit, MinOrderQuantity, StockQuantity, Origin, IsOrganic, IsActive, CreatedAt, UpdatedAt)
VALUES
('Chicken Breast', 'Meat', 'Fresh boneless chicken breast', 7.99, 'SKU-CHK001', 'KG', 10, 300, 'US', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Beef Ribeye', 'Meat', 'Premium ribeye steak', 24.99, 'SKU-BEF001', 'KG', 5, 100, 'US', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Ground Turkey', 'Meat', 'Lean ground turkey', 5.99, 'SKU-TRK001', 'KG', 10, 250, 'US', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Pork Chops', 'Meat', 'Center-cut pork chops', 8.99, 'SKU-PRK001', 'KG', 10, 200, 'DE', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Organic Lamb', 'Meat', 'Organic lamb shoulder', 18.99, 'SKU-LMB001', 'KG', 5, 50, 'NZ', 1, 1, GETUTCDATE(), GETUTCDATE());

-- Insert sample seafood
INSERT INTO Products (Name, Category, Description, Price, SKU, Unit, MinOrderQuantity, StockQuantity, Origin, IsOrganic, IsActive, CreatedAt, UpdatedAt)
VALUES
('Atlantic Salmon', 'Seafood', 'Fresh Atlantic salmon fillet', 14.99, 'SKU-SAL001', 'KG', 5, 150, 'NO', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Wild Tuna', 'Seafood', 'Wild-caught tuna steaks', 19.99, 'SKU-TUN001', 'KG', 5, 80, 'JP', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Fresh Shrimp', 'Seafood', 'Large fresh shrimp', 16.99, 'SKU-SHR001', 'KG', 10, 200, 'TH', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Sea Bass', 'Seafood', 'Mediterranean sea bass', 12.99, 'SKU-BAS001', 'KG', 5, 100, 'GR', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Lobster Tails', 'Seafood', 'Premium lobster tails', 34.99, 'SKU-LOB001', 'KG', 2, 30, 'CA', 0, 1, GETUTCDATE(), GETUTCDATE());

-- Insert sample dairy products
INSERT INTO Products (Name, Category, Description, Price, SKU, Unit, MinOrderQuantity, StockQuantity, Origin, IsOrganic, IsActive, CreatedAt, UpdatedAt)
VALUES
('Whole Milk', 'Dairy', 'Fresh whole milk', 3.49, 'SKU-MLK001', 'LB', 12, 500, 'US', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Organic Yogurt', 'Dairy', 'Organic Greek yogurt', 5.99, 'SKU-YOG001', 'CASE', 24, 300, 'GR', 1, 1, GETUTCDATE(), GETUTCDATE()),
('Cheddar Cheese', 'Dairy', 'Aged cheddar cheese', 8.99, 'SKU-CHE001', 'KG', 5, 200, 'GB', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Fresh Butter', 'Dairy', 'Premium fresh butter', 4.99, 'SKU-BUT001', 'KG', 10, 400, 'FR', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Organic Eggs', 'Dairy', 'Free-range organic eggs', 6.99, 'SKU-EGG001', 'CASE', 30, 1000, 'US', 1, 1, GETUTCDATE(), GETUTCDATE());

-- Insert sample grains
INSERT INTO Products (Name, Category, Description, Price, SKU, Unit, MinOrderQuantity, StockQuantity, Origin, IsOrganic, IsActive, CreatedAt, UpdatedAt)
VALUES
('Basmati Rice', 'Grains', 'Premium basmati rice', 3.99, 'SKU-RIC001', 'KG', 25, 1000, 'IN', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Whole Wheat Flour', 'Grains', 'Stone-ground whole wheat flour', 2.49, 'SKU-FLR001', 'KG', 20, 800, 'US', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Organic Quinoa', 'Grains', 'Organic white quinoa', 7.99, 'SKU-QUI001', 'KG', 10, 300, 'PE', 1, 1, GETUTCDATE(), GETUTCDATE()),
('Pasta Penne', 'Grains', 'Italian penne pasta', 2.99, 'SKU-PAS001', 'KG', 20, 600, 'IT', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Rolled Oats', 'Grains', 'Premium rolled oats', 3.49, 'SKU-OAT001', 'KG', 15, 500, 'CA', 0, 1, GETUTCDATE(), GETUTCDATE());

-- Insert sample oils
INSERT INTO Products (Name, Category, Description, Price, SKU, Unit, MinOrderQuantity, StockQuantity, Origin, IsOrganic, IsActive, CreatedAt, UpdatedAt)
VALUES
('Extra Virgin Olive Oil', 'Oils', 'Premium extra virgin olive oil', 12.99, 'SKU-OLV001', 'LB', 6, 200, 'IT', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Organic Coconut Oil', 'Oils', 'Cold-pressed organic coconut oil', 9.99, 'SKU-COC001', 'LB', 12, 150, 'PH', 1, 1, GETUTCDATE(), GETUTCDATE()),
('Sunflower Oil', 'Oils', 'Pure sunflower oil', 4.99, 'SKU-SUN001', 'LB', 12, 300, 'UA', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Avocado Oil', 'Oils', 'Premium avocado oil', 14.99, 'SKU-AVO001', 'LB', 6, 100, 'MX', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Sesame Oil', 'Oils', 'Pure sesame oil', 8.99, 'SKU-SES001', 'LB', 12, 150, 'JP', 0, 1, GETUTCDATE(), GETUTCDATE());

-- Insert sample beverages
INSERT INTO Products (Name, Category, Description, Price, SKU, Unit, MinOrderQuantity, StockQuantity, Origin, IsOrganic, IsActive, CreatedAt, UpdatedAt)
VALUES
('Orange Juice', 'Beverages', '100% pure orange juice', 4.99, 'SKU-OJU001', 'CASE', 12, 400, 'US', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Organic Green Tea', 'Beverages', 'Premium organic green tea', 6.99, 'SKU-TEA001', 'CASE', 24, 300, 'JP', 1, 1, GETUTCDATE(), GETUTCDATE()),
('Colombian Coffee', 'Beverages', 'Premium Colombian coffee beans', 11.99, 'SKU-COF001', 'KG', 5, 200, 'CO', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Sparkling Water', 'Beverages', 'Natural sparkling water', 2.99, 'SKU-WAT001', 'CASE', 24, 800, 'FR', 0, 1, GETUTCDATE(), GETUTCDATE()),
('Organic Kombucha', 'Beverages', 'Organic fermented kombucha', 5.99, 'SKU-KOM001', 'CASE', 12, 150, 'US', 1, 1, GETUTCDATE(), GETUTCDATE());

-- Display summary
SELECT 'Products Added!' as Status, COUNT(*) as TotalProducts, COUNT(DISTINCT Category) as Categories FROM Products;
SELECT Category, COUNT(*) as ProductCount, AVG(Price) as AvgPrice, MIN(Price) as MinPrice, MAX(Price) as MaxPrice 
FROM Products 
GROUP BY Category 
ORDER BY Category;
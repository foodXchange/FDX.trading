-- Check if buyers exist
IF NOT EXISTS (SELECT 1 FROM FoodXBuyers WHERE Company IS NOT NULL)
BEGIN
    PRINT 'No buyers found. Adding sample buyers...'
    
    -- Add sample buyers
    INSERT INTO FoodXBuyers (Company, Type, Region, ProcurementEmail, Country)
    VALUES 
        ('Walmart', 'Retail', 'North America', 'procurement@walmart.com', 'USA'),
        ('Carrefour', 'Retail', 'Europe', 'buying@carrefour.com', 'France'),
        ('Tesco', 'Retail', 'Europe', 'procurement@tesco.com', 'UK'),
        ('Metro AG', 'Wholesale', 'Europe', 'sourcing@metro.de', 'Germany'),
        ('Costco', 'Wholesale', 'North America', 'buyers@costco.com', 'USA'),
        ('Amazon Fresh', 'E-commerce', 'Global', 'fresh@amazon.com', 'USA'),
        ('Kroger', 'Retail', 'North America', 'procurement@kroger.com', 'USA'),
        ('Aldi', 'Retail', 'Europe', 'buying@aldi.com', 'Germany'),
        ('Lidl', 'Retail', 'Europe', 'sourcing@lidl.com', 'Germany'),
        ('Whole Foods', 'Retail', 'North America', 'buyers@wholefoods.com', 'USA')
    
    PRINT 'Added 10 sample buyers'
END
ELSE
BEGIN
    PRINT 'Buyers already exist in database'
    
    -- Show count and sample
    SELECT COUNT(*) as TotalBuyers FROM FoodXBuyers WHERE Company IS NOT NULL
    
    SELECT TOP 10 Id, Company, Type, Region, Country 
    FROM FoodXBuyers 
    WHERE Company IS NOT NULL 
    ORDER BY Company
END
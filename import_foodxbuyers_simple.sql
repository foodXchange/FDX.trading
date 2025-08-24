-- Simple import of FoodXBuyers data to Companies table
-- This script migrates buyer company data from the FoodXBuyers table to the main Companies table

BEGIN TRANSACTION;

-- Check how many new companies we'll be adding
DECLARE @TotalBuyers INT, @ExistingCompanies INT, @NewCompanies INT;

SELECT @TotalBuyers = COUNT(*) FROM FoodXBuyers WHERE Company IS NOT NULL AND Company != '';
SELECT @ExistingCompanies = COUNT(*) 
FROM FoodXBuyers fb 
INNER JOIN Companies c ON c.Name = fb.Company
WHERE fb.Company IS NOT NULL AND fb.Company != '';

SET @NewCompanies = @TotalBuyers - @ExistingCompanies;

PRINT 'Migration Summary:';
PRINT 'Total valid FoodXBuyers: ' + CAST(@TotalBuyers AS VARCHAR(10));
PRINT 'Already in Companies: ' + CAST(@ExistingCompanies AS VARCHAR(10));
PRINT 'New companies to add: ' + CAST(@NewCompanies AS VARCHAR(10));

-- Insert new companies from FoodXBuyers
INSERT INTO Companies (
    Name,
    CompanyType,
    BuyerCategory,
    Country,
    Website,
    MainEmail,
    MainPhone,
    Address
)
SELECT DISTINCT
    fb.Company AS Name,
    'Buyer' AS CompanyType,
    fb.Type AS BuyerCategory,
    fb.Region AS Country,
    fb.Website,
    COALESCE(fb.ProcurementEmail, fb.ContactEmail) AS MainEmail,
    COALESCE(fb.Phone, fb.Mobile) AS MainPhone,
    fb.Address
FROM FoodXBuyers fb
WHERE NOT EXISTS (
    SELECT 1 FROM Companies c WHERE c.Name = fb.Company
)
AND fb.Company IS NOT NULL
AND fb.Company != '';

DECLARE @InsertedCount INT = @@ROWCOUNT;
PRINT 'Successfully inserted ' + CAST(@InsertedCount AS VARCHAR(10)) + ' new companies.';

-- Show summary
SELECT 
    'Import Summary' AS Report,
    COUNT(*) AS TotalBuyerCompanies
FROM Companies 
WHERE CompanyType = 'Buyer';

-- Show breakdown by category
SELECT 
    ISNULL(BuyerCategory, 'Unknown') AS Category,
    COUNT(*) AS Count
FROM Companies 
WHERE CompanyType = 'Buyer'
GROUP BY BuyerCategory
ORDER BY COUNT(*) DESC;

COMMIT TRANSACTION;

PRINT 'Migration completed successfully!';
-- Minimal import of FoodXBuyers data to Companies table
-- Only using confirmed existing columns

BEGIN TRANSACTION;

-- Check current state
DECLARE @TotalBuyers INT, @ExistingCompanies INT;

SELECT @TotalBuyers = COUNT(*) FROM FoodXBuyers WHERE Company IS NOT NULL AND Company != '';
SELECT @ExistingCompanies = COUNT(*) FROM Companies WHERE CompanyType = 'Buyer';

PRINT 'Pre-Import Status:';
PRINT 'Total FoodXBuyers: ' + CAST(@TotalBuyers AS VARCHAR(10));
PRINT 'Existing in Companies: ' + CAST(@ExistingCompanies AS VARCHAR(10));

-- Insert new companies from FoodXBuyers (minimal fields)
INSERT INTO Companies (
    Name,
    CompanyType,
    BuyerCategory,
    Country,
    Website,
    MainEmail
)
SELECT DISTINCT
    fb.Company AS Name,
    'Buyer' AS CompanyType,
    fb.Type AS BuyerCategory,
    fb.Region AS Country,
    fb.Website,
    COALESCE(fb.ProcurementEmail, fb.GeneralEmail) AS MainEmail
FROM FoodXBuyers fb
WHERE NOT EXISTS (
    SELECT 1 FROM Companies c WHERE c.Name = fb.Company
)
AND fb.Company IS NOT NULL
AND fb.Company != ''
AND fb.Company != 'NULL';

DECLARE @InsertedCount INT = @@ROWCOUNT;
PRINT 'Successfully inserted ' + CAST(@InsertedCount AS VARCHAR(10)) + ' new companies.';

-- Show final count
SELECT COUNT(*) AS TotalBuyerCompanies FROM Companies WHERE CompanyType = 'Buyer';

-- Show breakdown by category
SELECT 
    ISNULL(BuyerCategory, 'Unknown') AS Category,
    COUNT(*) AS Count
FROM Companies 
WHERE CompanyType = 'Buyer'
GROUP BY BuyerCategory
ORDER BY COUNT(*) DESC;

COMMIT TRANSACTION;

PRINT 'Import completed successfully!';
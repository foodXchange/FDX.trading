-- Import FoodXBuyers data to Companies table (v2 - Fixed column names)
-- This script migrates buyer company data from the FoodXBuyers table to the main Companies table

BEGIN TRANSACTION;

-- First, check how many records we'll be importing
DECLARE @TotalBuyers INT, @ExistingCompanies INT, @NewCompanies INT;

SELECT @TotalBuyers = COUNT(*) FROM FoodXBuyers;
SELECT @ExistingCompanies = COUNT(*) 
FROM FoodXBuyers fb 
INNER JOIN Companies c ON c.Name = fb.Company;

SET @NewCompanies = @TotalBuyers - @ExistingCompanies;

PRINT 'Migration Summary:';
PRINT '==================';
PRINT 'Total FoodXBuyers: ' + CAST(@TotalBuyers AS VARCHAR(10));
PRINT 'Already in Companies: ' + CAST(@ExistingCompanies AS VARCHAR(10));
PRINT 'New companies to add: ' + CAST(@NewCompanies AS VARCHAR(10));
PRINT '';

-- Insert new companies from FoodXBuyers that don't already exist
INSERT INTO Companies (
    Name,
    CompanyType,
    BuyerCategory,
    Country,
    Website,
    MainEmail,
    MainPhone,
    Address,
    IsActive,
    CreatedAt,
    UpdatedAt
)
SELECT 
    fb.Company AS Name,
    'Buyer' AS CompanyType,
    fb.Type AS BuyerCategory,  -- Hotel, Restaurant, Retail Chain, etc.
    fb.Region AS Country,       -- Using Region as Country
    fb.Website,
    COALESCE(fb.ProcurementEmail, fb.ContactEmail) AS MainEmail,
    COALESCE(fb.Phone, fb.Mobile) AS MainPhone,
    fb.Address,
    1 AS IsActive,
    GETUTCDATE() AS CreatedAt,
    GETUTCDATE() AS UpdatedAt
FROM FoodXBuyers fb
WHERE NOT EXISTS (
    SELECT 1 FROM Companies c WHERE c.Name = fb.Company
)
AND fb.Company IS NOT NULL
AND fb.Company != '';

-- Get count of inserted records
DECLARE @InsertedCount INT = @@ROWCOUNT;
PRINT 'Successfully inserted ' + CAST(@InsertedCount AS VARCHAR(10)) + ' new companies.';

-- Create a mapping table to track which FoodXBuyer records have been imported
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FoodXBuyerImportLog')
BEGIN
    CREATE TABLE FoodXBuyerImportLog (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FoodXBuyerId INT NOT NULL,
        CompanyId INT NOT NULL,
        ImportedAt DATETIME2 DEFAULT GETUTCDATE(),
        ImportedBy NVARCHAR(256) DEFAULT SYSTEM_USER,
        Notes NVARCHAR(MAX),
        FOREIGN KEY (FoodXBuyerId) REFERENCES FoodXBuyers(Id),
        FOREIGN KEY (CompanyId) REFERENCES Companies(Id)
    );
    PRINT 'Created FoodXBuyerImportLog table for tracking imports.';
END

-- Log the imported records with additional metadata
INSERT INTO FoodXBuyerImportLog (FoodXBuyerId, CompanyId, Notes)
SELECT 
    fb.Id AS FoodXBuyerId,
    c.Id AS CompanyId,
    CONCAT(
        'Categories: ', ISNULL(fb.Categories, 'N/A'),
        ' | Markets: ', ISNULL(fb.Markets, 'N/A'),
        CASE WHEN fb.Size IS NOT NULL THEN ' | Size: ' + fb.Size ELSE '' END,
        CASE WHEN fb.Stores IS NOT NULL THEN ' | Stores: ' + fb.Stores ELSE '' END,
        CASE WHEN fb.KeyContact IS NOT NULL THEN ' | Contact: ' + fb.KeyContact ELSE '' END,
        CASE WHEN fb.ProcurementContact IS NOT NULL THEN ' | Procurement: ' + fb.ProcurementContact ELSE '' END
    ) AS Notes
FROM FoodXBuyers fb
INNER JOIN Companies c ON c.Name = fb.Company
WHERE NOT EXISTS (
    SELECT 1 FROM FoodXBuyerImportLog log 
    WHERE log.FoodXBuyerId = fb.Id
);

DECLARE @LoggedCount INT = @@ROWCOUNT;
PRINT 'Logged ' + CAST(@LoggedCount AS VARCHAR(10)) + ' import mappings.';

-- Show summary of imported data
PRINT '';
PRINT 'Import Results by Buyer Category:';
PRINT '==================================';
SELECT 
    ISNULL(c.BuyerCategory, 'Unknown') AS Category,
    COUNT(*) AS Count
FROM Companies c
WHERE c.CompanyType = 'Buyer'
GROUP BY c.BuyerCategory
ORDER BY COUNT(*) DESC;

-- Show sample of imported companies
PRINT '';
PRINT 'Sample of Imported Companies:';
SELECT TOP 5
    c.Id,
    c.Name,
    c.BuyerCategory,
    c.MainEmail,
    c.Country
FROM Companies c
WHERE c.CompanyType = 'Buyer'
ORDER BY c.CreatedAt DESC;

-- Verify data integrity
DECLARE @CompanyCount INT, @BuyerCompanyCount INT;
SELECT @CompanyCount = COUNT(*) FROM Companies;
SELECT @BuyerCompanyCount = COUNT(*) FROM Companies WHERE CompanyType = 'Buyer';

PRINT '';
PRINT 'Final Database State:';
PRINT '=====================';
PRINT 'Total Companies: ' + CAST(@CompanyCount AS VARCHAR(10));
PRINT 'Buyer Companies: ' + CAST(@BuyerCompanyCount AS VARCHAR(10));
PRINT '';

-- Commit the transaction
COMMIT TRANSACTION;

PRINT 'Migration completed successfully!';
PRINT 'You can now use the Companies table for all buyer company operations.';
PRINT '';
PRINT 'Note: Additional buyer details (categories, stores, contacts) have been preserved in FoodXBuyerImportLog.';
-- Import FoodXBuyers data to Companies table
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
    Website,
    Email,
    Phone,
    Address,
    Country,
    City,
    ContactPerson,
    Industry,
    Size,
    Description,
    IsActive,
    CreatedAt,
    UpdatedAt
)
SELECT 
    fb.Company AS Name,
    'Buyer' AS CompanyType,
    fb.Website,
    COALESCE(fb.ProcurementEmail, fb.ContactEmail) AS Email,
    COALESCE(fb.Phone, fb.Mobile) AS Phone,
    fb.Address,
    fb.Region AS Country,  -- Using Region as Country for now
    fb.Markets AS City,     -- Using Markets as City for now
    COALESCE(fb.ProcurementContact, fb.KeyContact) AS ContactPerson,
    fb.Type AS Industry,    -- Hotel, Restaurant, Retail Chain, etc.
    fb.Size,
    CONCAT(
        'Type: ', ISNULL(fb.Type, 'N/A'), 
        ' | Categories: ', ISNULL(fb.Categories, 'N/A'),
        CASE WHEN fb.Stores IS NOT NULL THEN ' | Stores: ' + fb.Stores ELSE '' END,
        CASE WHEN fb.DistributionCenter IS NOT NULL THEN ' | DC: ' + fb.DistributionCenter ELSE '' END,
        CASE WHEN fb.Notes IS NOT NULL THEN ' | Notes: ' + fb.Notes ELSE '' END
    ) AS Description,
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
        FOREIGN KEY (FoodXBuyerId) REFERENCES FoodXBuyers(Id),
        FOREIGN KEY (CompanyId) REFERENCES Companies(Id)
    );
    PRINT 'Created FoodXBuyerImportLog table for tracking imports.';
END

-- Log the imported records
INSERT INTO FoodXBuyerImportLog (FoodXBuyerId, CompanyId)
SELECT 
    fb.Id AS FoodXBuyerId,
    c.Id AS CompanyId
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
PRINT 'Import Results by Type:';
PRINT '=======================';
SELECT 
    c.Industry AS Type,
    COUNT(*) AS Count
FROM Companies c
WHERE c.CompanyType = 'Buyer'
GROUP BY c.Industry
ORDER BY COUNT(*) DESC;

-- Show sample of imported companies
PRINT '';
PRINT 'Sample of Imported Companies:';
SELECT TOP 5
    c.Id,
    c.Name,
    c.Industry AS Type,
    c.Email,
    c.Country AS Region
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
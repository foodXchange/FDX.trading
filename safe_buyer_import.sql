-- Safe buyer import with duplicate handling

-- First, show what we're about to import
SELECT COUNT(DISTINCT Company) as UniqueCompaniesToImport
FROM FoodXBuyers 
WHERE Company IS NOT NULL 
AND Company != ''
AND Company NOT IN (SELECT Name FROM Companies WHERE Name IS NOT NULL);

-- Import with ROW_NUMBER to handle duplicates
WITH UniqueB AS (
    SELECT 
        Company,
        Type,
        Region,
        Website,
        ProcurementEmail,
        ROW_NUMBER() OVER (PARTITION BY Company ORDER BY Id) as rn
    FROM FoodXBuyers
    WHERE Company IS NOT NULL 
    AND Company != ''
)
INSERT INTO Companies (Name, CompanyType, BuyerCategory, Country, Website, MainEmail)
SELECT 
    Company AS Name,
    'Buyer' AS CompanyType,
    Type AS BuyerCategory,
    Region AS Country,
    Website,
    ProcurementEmail AS MainEmail
FROM UniqueB
WHERE rn = 1
AND Company NOT IN (SELECT Name FROM Companies WHERE Name IS NOT NULL);

-- Verify results
SELECT COUNT(*) as TotalBuyerCompanies FROM Companies WHERE CompanyType = 'Buyer';
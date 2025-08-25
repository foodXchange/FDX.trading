-- Direct batch import of buyers

-- First, let's see what we're about to import
SELECT TOP 20 
    Company,
    Type,
    Region,
    Website,
    ProcurementEmail
FROM FoodXBuyers 
WHERE Company IS NOT NULL 
AND Company != ''
AND Company NOT IN (SELECT Name FROM Companies)
ORDER BY Company;

-- Now do the actual import
INSERT INTO Companies (Name, CompanyType, BuyerCategory, Country, Website, MainEmail)
SELECT DISTINCT
    Company AS Name,
    'Buyer' AS CompanyType,
    Type AS BuyerCategory,
    Region AS Country,
    Website,
    ProcurementEmail AS MainEmail
FROM FoodXBuyers
WHERE Company IS NOT NULL 
AND Company != ''
AND Company NOT IN (SELECT Name FROM Companies);

-- Show results
SELECT COUNT(*) as TotalBuyersInCompanies FROM Companies WHERE CompanyType = 'Buyer';
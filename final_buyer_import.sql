-- Final buyer import - simplified version
-- Import all FoodXBuyers that aren't already in Companies

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
AND Company NOT IN (SELECT Name FROM Companies WHERE Name IS NOT NULL);
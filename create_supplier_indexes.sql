-- Create indexes for FoodXSuppliers table to improve search performance
-- Author: Claude Code
-- Date: 2025-08-26

-- Index for supplier name searches
CREATE NONCLUSTERED INDEX IX_FoodXSuppliers_SupplierName 
ON FoodXSuppliers(SupplierName)
INCLUDE (Country, ProductCategory, Products, CompanyEmail);

-- Index for country filtering
CREATE NONCLUSTERED INDEX IX_FoodXSuppliers_Country 
ON FoodXSuppliers(Country)
WHERE Country IS NOT NULL;

-- Index for category filtering  
CREATE NONCLUSTERED INDEX IX_FoodXSuppliers_ProductCategory
ON FoodXSuppliers(ProductCategory)
WHERE ProductCategory IS NOT NULL;

-- Composite index for common search patterns
CREATE NONCLUSTERED INDEX IX_FoodXSuppliers_Search
ON FoodXSuppliers(Country, ProductCategory)
INCLUDE (SupplierName, Products, CompanyEmail, Description);

-- Add computed columns for certifications (for better performance)
ALTER TABLE FoodXSuppliers ADD IsKosherCertified AS 
    CAST(CASE 
        WHEN KosherCertification IS NOT NULL AND KosherCertification != '' THEN 1
        WHEN Products LIKE '%kosher%' THEN 1
        WHEN Country = 'Israel' THEN 1
        ELSE 0
    END AS BIT);

ALTER TABLE FoodXSuppliers ADD IsHalalCertified AS
    CAST(CASE
        WHEN Products LIKE '%halal%' THEN 1
        WHEN Certifications LIKE '%halal%' THEN 1
        ELSE 0
    END AS BIT);

ALTER TABLE FoodXSuppliers ADD IsOrganicCertified AS
    CAST(CASE
        WHEN Products LIKE '%organic%' THEN 1
        WHEN Certifications LIKE '%organic%' THEN 1
        ELSE 0
    END AS BIT);

ALTER TABLE FoodXSuppliers ADD IsGlutenFreeCertified AS
    CAST(CASE
        WHEN Products LIKE '%gluten-free%' OR Products LIKE '%gluten free%' THEN 1
        ELSE 0
    END AS BIT);

-- Index the computed columns (separate indexes for better performance)
CREATE NONCLUSTERED INDEX IX_FoodXSuppliers_IsKosher
ON FoodXSuppliers(IsKosherCertified)
WHERE IsKosherCertified = 1;

CREATE NONCLUSTERED INDEX IX_FoodXSuppliers_IsHalal
ON FoodXSuppliers(IsHalalCertified)
WHERE IsHalalCertified = 1;

CREATE NONCLUSTERED INDEX IX_FoodXSuppliers_IsOrganic
ON FoodXSuppliers(IsOrganicCertified)
WHERE IsOrganicCertified = 1;

CREATE NONCLUSTERED INDEX IX_FoodXSuppliers_IsGlutenFree
ON FoodXSuppliers(IsGlutenFreeCertified)
WHERE IsGlutenFreeCertified = 1;

-- Update statistics for better query plans
UPDATE STATISTICS FoodXSuppliers;

PRINT 'Supplier indexes and computed columns created successfully';
-- Step 2: Update certification values based on existing data
-- Author: Claude Code
-- Date: 2025-08-26

-- Update IsKosherCertified
UPDATE FoodXSuppliers
SET IsKosherCertified = CASE
    WHEN KosherCertification IS NOT NULL AND KosherCertification != '' THEN 1
    WHEN Products LIKE '%kosher%' THEN 1
    WHEN Country = 'Israel' THEN 1
    ELSE 0
END
WHERE IsKosherCertified IS NULL;

-- Update IsHalalCertified
UPDATE FoodXSuppliers
SET IsHalalCertified = CASE
    WHEN Products LIKE '%halal%' THEN 1
    WHEN Certifications LIKE '%halal%' THEN 1
    ELSE 0
END
WHERE IsHalalCertified IS NULL;

-- Update IsOrganicCertified
UPDATE FoodXSuppliers
SET IsOrganicCertified = CASE
    WHEN Products LIKE '%organic%' THEN 1
    WHEN Certifications LIKE '%organic%' THEN 1
    ELSE 0
END
WHERE IsOrganicCertified IS NULL;

-- Update IsGlutenFreeCertified
UPDATE FoodXSuppliers
SET IsGlutenFreeCertified = CASE
    WHEN Products LIKE '%gluten-free%' OR Products LIKE '%gluten free%' THEN 1
    ELSE 0
END
WHERE IsGlutenFreeCertified IS NULL;

-- Show counts
SELECT 
    COUNT(*) as TotalSuppliers,
    SUM(CASE WHEN IsKosherCertified = 1 THEN 1 ELSE 0 END) as KosherSuppliers,
    SUM(CASE WHEN IsHalalCertified = 1 THEN 1 ELSE 0 END) as HalalSuppliers,
    SUM(CASE WHEN IsOrganicCertified = 1 THEN 1 ELSE 0 END) as OrganicSuppliers,
    SUM(CASE WHEN IsGlutenFreeCertified = 1 THEN 1 ELSE 0 END) as GlutenFreeSuppliers
FROM FoodXSuppliers;

PRINT 'Certification values updated successfully';
-- Add certification boolean columns to FoodXSuppliers table
-- Author: Claude Code
-- Date: 2025-08-26

-- Add IsKosherCertified column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FoodXSuppliers') AND name = 'IsKosherCertified')
BEGIN
    ALTER TABLE FoodXSuppliers 
    ADD IsKosherCertified BIT NULL;
    
    -- Update existing records based on existing data
    UPDATE FoodXSuppliers
    SET IsKosherCertified = CASE
        WHEN KosherCertification IS NOT NULL AND KosherCertification != '' THEN 1
        WHEN Products LIKE '%kosher%' THEN 1
        WHEN Country = 'Israel' THEN 1
        ELSE 0
    END;
END

-- Add IsHalalCertified column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FoodXSuppliers') AND name = 'IsHalalCertified')
BEGIN
    ALTER TABLE FoodXSuppliers 
    ADD IsHalalCertified BIT NULL;
    
    -- Update existing records based on existing data
    UPDATE FoodXSuppliers
    SET IsHalalCertified = CASE
        WHEN Products LIKE '%halal%' THEN 1
        WHEN Certifications LIKE '%halal%' THEN 1
        ELSE 0
    END;
END

-- Add IsOrganicCertified column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FoodXSuppliers') AND name = 'IsOrganicCertified')
BEGIN
    ALTER TABLE FoodXSuppliers 
    ADD IsOrganicCertified BIT NULL;
    
    -- Update existing records based on existing data
    UPDATE FoodXSuppliers
    SET IsOrganicCertified = CASE
        WHEN Products LIKE '%organic%' THEN 1
        WHEN Certifications LIKE '%organic%' THEN 1
        ELSE 0
    END;
END

-- Add IsGlutenFreeCertified column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FoodXSuppliers') AND name = 'IsGlutenFreeCertified')
BEGIN
    ALTER TABLE FoodXSuppliers 
    ADD IsGlutenFreeCertified BIT NULL;
    
    -- Update existing records based on existing data
    UPDATE FoodXSuppliers
    SET IsGlutenFreeCertified = CASE
        WHEN Products LIKE '%gluten-free%' OR Products LIKE '%gluten free%' THEN 1
        ELSE 0
    END;
END

-- Create indexes on the new certification columns for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodXSuppliers_Certifications_Combined')
BEGIN
    CREATE NONCLUSTERED INDEX IX_FoodXSuppliers_Certifications_Combined
    ON FoodXSuppliers(IsKosherCertified, IsHalalCertified, IsOrganicCertified, IsGlutenFreeCertified)
    INCLUDE (SupplierName, Country, ProductCategory);
END

-- Update statistics
UPDATE STATISTICS FoodXSuppliers;

PRINT 'Certification columns added successfully';
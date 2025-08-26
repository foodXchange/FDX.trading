-- Step 1: Add certification boolean columns to FoodXSuppliers table
-- Author: Claude Code
-- Date: 2025-08-26

-- Add IsKosherCertified column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FoodXSuppliers') AND name = 'IsKosherCertified')
BEGIN
    ALTER TABLE FoodXSuppliers 
    ADD IsKosherCertified BIT NULL;
END

-- Add IsHalalCertified column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FoodXSuppliers') AND name = 'IsHalalCertified')
BEGIN
    ALTER TABLE FoodXSuppliers 
    ADD IsHalalCertified BIT NULL;
END

-- Add IsOrganicCertified column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FoodXSuppliers') AND name = 'IsOrganicCertified')
BEGIN
    ALTER TABLE FoodXSuppliers 
    ADD IsOrganicCertified BIT NULL;
END

-- Add IsGlutenFreeCertified column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FoodXSuppliers') AND name = 'IsGlutenFreeCertified')
BEGIN
    ALTER TABLE FoodXSuppliers 
    ADD IsGlutenFreeCertified BIT NULL;
END

PRINT 'Certification columns added successfully';
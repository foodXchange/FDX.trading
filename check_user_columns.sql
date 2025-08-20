-- Check columns in AspNetUsers table
USE FoodXDB;
GO

-- Show all columns in AspNetUsers table
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AspNetUsers'
ORDER BY ORDINAL_POSITION;

-- Add missing columns if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'FirstName')
BEGIN
    ALTER TABLE AspNetUsers ADD FirstName NVARCHAR(100) NULL;
    PRINT 'Added FirstName column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'LastName')
BEGIN
    ALTER TABLE AspNetUsers ADD LastName NVARCHAR(100) NULL;
    PRINT 'Added LastName column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'CompanyName')
BEGIN
    ALTER TABLE AspNetUsers ADD CompanyName NVARCHAR(200) NULL;
    PRINT 'Added CompanyName column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'InvitationCode')
BEGIN
    ALTER TABLE AspNetUsers ADD InvitationCode NVARCHAR(100) NULL;
    PRINT 'Added InvitationCode column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'InvitationId')
BEGIN
    ALTER TABLE AspNetUsers ADD InvitationId INT NULL;
    PRINT 'Added InvitationId column';
END

-- Show updated columns
PRINT '';
PRINT 'Updated AspNetUsers columns:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AspNetUsers'
ORDER BY ORDINAL_POSITION;
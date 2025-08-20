-- Fix AspNetUsers Schema for FoodX
USE FoodXDB;
GO

-- Add all missing columns needed by ApplicationUser
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'FirstName')
    ALTER TABLE AspNetUsers ADD FirstName NVARCHAR(100) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'LastName')
    ALTER TABLE AspNetUsers ADD LastName NVARCHAR(100) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'CompanyName')
    ALTER TABLE AspNetUsers ADD CompanyName NVARCHAR(200) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Department')
    ALTER TABLE AspNetUsers ADD Department NVARCHAR(100) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'JobTitle')
    ALTER TABLE AspNetUsers ADD JobTitle NVARCHAR(100) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'PhoneExtension')
    ALTER TABLE AspNetUsers ADD PhoneExtension NVARCHAR(10) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'InvitationCode')
    ALTER TABLE AspNetUsers ADD InvitationCode NVARCHAR(100) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'InvitationId')
    ALTER TABLE AspNetUsers ADD InvitationId INT NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'CreatedAt')
    ALTER TABLE AspNetUsers ADD CreatedAt DATETIME2 NULL DEFAULT GETUTCDATE();

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'UpdatedAt')
    ALTER TABLE AspNetUsers ADD UpdatedAt DATETIME2 NULL DEFAULT GETUTCDATE();

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'LastLoginAt')
    ALTER TABLE AspNetUsers ADD LastLoginAt DATETIME2 NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'IsActive')
    ALTER TABLE AspNetUsers ADD IsActive BIT NULL DEFAULT 1;

PRINT 'AspNetUsers schema updated successfully';

-- Verify all columns
SELECT 
    c.COLUMN_NAME,
    c.DATA_TYPE,
    c.CHARACTER_MAXIMUM_LENGTH,
    c.IS_NULLABLE,
    c.COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.TABLE_NAME = 'AspNetUsers'
ORDER BY c.ORDINAL_POSITION;
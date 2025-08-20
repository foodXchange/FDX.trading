-- Comprehensive column creation for AspNetUsers
USE FoodXDB;
GO

-- Check if table exists
IF OBJECT_ID('dbo.AspNetUsers', 'U') IS NULL
BEGIN
    PRINT 'ERROR: AspNetUsers table does not exist!';
    RETURN;
END

PRINT 'Adding missing columns to AspNetUsers table...';

-- Add each column one by one
IF COL_LENGTH('dbo.AspNetUsers', 'FirstName') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD FirstName NVARCHAR(100) NULL;
    PRINT 'Added FirstName';
END
ELSE
    PRINT 'FirstName already exists';

IF COL_LENGTH('dbo.AspNetUsers', 'LastName') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD LastName NVARCHAR(100) NULL;
    PRINT 'Added LastName';
END
ELSE
    PRINT 'LastName already exists';

IF COL_LENGTH('dbo.AspNetUsers', 'CompanyName') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD CompanyName NVARCHAR(200) NULL;
    PRINT 'Added CompanyName';
END
ELSE
    PRINT 'CompanyName already exists';

IF COL_LENGTH('dbo.AspNetUsers', 'Department') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD Department NVARCHAR(100) NULL;
    PRINT 'Added Department';
END
ELSE
    PRINT 'Department already exists';

IF COL_LENGTH('dbo.AspNetUsers', 'JobTitle') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD JobTitle NVARCHAR(100) NULL;
    PRINT 'Added JobTitle';
END
ELSE
    PRINT 'JobTitle already exists';

IF COL_LENGTH('dbo.AspNetUsers', 'IsSuperAdmin') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD IsSuperAdmin BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsSuperAdmin';
END
ELSE
    PRINT 'IsSuperAdmin already exists';

IF COL_LENGTH('dbo.AspNetUsers', 'ImpersonatedBy') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD ImpersonatedBy NVARCHAR(450) NULL;
    PRINT 'Added ImpersonatedBy';
END
ELSE
    PRINT 'ImpersonatedBy already exists';

IF COL_LENGTH('dbo.AspNetUsers', 'LastLoginAt') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD LastLoginAt DATETIME2 NULL;
    PRINT 'Added LastLoginAt';
END
ELSE
    PRINT 'LastLoginAt already exists';

IF COL_LENGTH('dbo.AspNetUsers', 'CreatedAt') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD CreatedAt DATETIME2 NULL;
    PRINT 'Added CreatedAt';
END
ELSE
    PRINT 'CreatedAt already exists';

IF COL_LENGTH('dbo.AspNetUsers', 'UpdatedAt') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD UpdatedAt DATETIME2 NULL;
    PRINT 'Added UpdatedAt';
END
ELSE
    PRINT 'UpdatedAt already exists';

IF COL_LENGTH('dbo.AspNetUsers', 'InvitationCode') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD InvitationCode NVARCHAR(100) NULL;
    PRINT 'Added InvitationCode';
END
ELSE
    PRINT 'InvitationCode already exists';

IF COL_LENGTH('dbo.AspNetUsers', 'InvitationId') IS NULL
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD InvitationId INT NULL;
    PRINT 'Added InvitationId';
END
ELSE
    PRINT 'InvitationId already exists';

PRINT '';
PRINT 'Verification - Current columns in AspNetUsers:';
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable
FROM sys.columns c
JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.AspNetUsers')
ORDER BY c.column_id;

PRINT '';
PRINT 'Column creation complete!';
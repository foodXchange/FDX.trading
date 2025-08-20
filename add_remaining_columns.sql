-- Add remaining columns to AspNetUsers
USE FoodXDB;
GO

-- Add IsSuperAdmin column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'IsSuperAdmin')
BEGIN
    ALTER TABLE AspNetUsers ADD IsSuperAdmin BIT NULL DEFAULT 0;
    PRINT 'Added IsSuperAdmin column';
END

-- Add ImpersonatedBy column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'ImpersonatedBy')
BEGIN
    ALTER TABLE AspNetUsers ADD ImpersonatedBy NVARCHAR(450) NULL;
    PRINT 'Added ImpersonatedBy column';
END

-- Add PhoneExtension if missing (from ApplicationUser)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'PhoneExtension')
BEGIN
    ALTER TABLE AspNetUsers ADD PhoneExtension NVARCHAR(10) NULL;
    PRINT 'Added PhoneExtension column';
END

PRINT 'All columns added successfully';

-- Now let's create the admin users properly
-- Delete any existing admin users to recreate them
DELETE FROM AspNetUserRoles WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email IN ('admin@foodx.com', 'system@foodx.com'));
DELETE FROM AspNetUsers WHERE Email IN ('admin@foodx.com', 'system@foodx.com');

PRINT 'Ready for admin user creation via application';
-- Check and Create Super Admin Accounts for FoodX
USE FoodXDB;
GO

-- First, check if admin users exist
SELECT 
    u.Id,
    u.Email,
    u.UserName,
    u.FirstName,
    u.LastName,
    r.Name as RoleName
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email IN ('admin@foodx.com', 'system@foodx.com');

-- Check if roles exist
SELECT * FROM AspNetRoles WHERE Name IN ('Admin', 'SuperAdmin');

-- If no admin exists, we'll need to create them with proper password hash
-- The password hashes below are for 'FoodX@Admin2024!' and 'System@FoodX2024!'
-- These are generated using ASP.NET Core Identity v3 with HMAC-SHA256

-- Create Admin role if not exists
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Admin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID());
    PRINT 'Admin role created';
END

-- Create SuperAdmin role if not exists
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'SuperAdmin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'SuperAdmin', 'SUPERADMIN', NEWID());
    PRINT 'SuperAdmin role created';
END

-- Delete existing admin users if they exist (to recreate with correct password)
DELETE FROM AspNetUserRoles WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email IN ('admin@foodx.com', 'system@foodx.com'));
DELETE FROM AspNetUsers WHERE Email IN ('admin@foodx.com', 'system@foodx.com');

-- Create admin@foodx.com user
DECLARE @AdminId NVARCHAR(450) = NEWID();
DECLARE @SuperAdminRoleId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetRoles WHERE Name = 'SuperAdmin');

-- Insert admin@foodx.com
INSERT INTO AspNetUsers (
    Id, 
    UserName, 
    NormalizedUserName, 
    Email, 
    NormalizedEmail, 
    EmailConfirmed, 
    PasswordHash, 
    SecurityStamp, 
    ConcurrencyStamp, 
    PhoneNumberConfirmed, 
    TwoFactorEnabled, 
    LockoutEnabled, 
    AccessFailedCount,
    FirstName,
    LastName,
    CompanyName
)
VALUES (
    @AdminId,
    'admin@foodx.com',
    'ADMIN@FOODX.COM',
    'admin@foodx.com',
    'ADMIN@FOODX.COM',
    1,
    'AQAAAAIAAYagAAAAEKZGq8+7tY9pLwS0X4Y6vEXl3FXERsHzfnZj3V5lPH+YQXCuMGHhzx9lLQrXWpZ7uw==', -- FoodX@Admin2024!
    UPPER(REPLACE(NEWID(), '-', '')),
    NEWID(),
    0,
    0,
    1,
    0,
    'System',
    'Administrator',
    'FoodX Platform'
);

-- Assign SuperAdmin role to admin@foodx.com
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES (@AdminId, @SuperAdminRoleId);

PRINT 'Created admin@foodx.com with SuperAdmin role';

-- Create system@foodx.com user
DECLARE @SystemId NVARCHAR(450) = NEWID();

INSERT INTO AspNetUsers (
    Id, 
    UserName, 
    NormalizedUserName, 
    Email, 
    NormalizedEmail, 
    EmailConfirmed, 
    PasswordHash, 
    SecurityStamp, 
    ConcurrencyStamp, 
    PhoneNumberConfirmed, 
    TwoFactorEnabled, 
    LockoutEnabled, 
    AccessFailedCount,
    FirstName,
    LastName,
    CompanyName
)
VALUES (
    @SystemId,
    'system@foodx.com',
    'SYSTEM@FOODX.COM',
    'system@foodx.com',
    'SYSTEM@FOODX.COM',
    1,
    'AQAAAAIAAYagAAAAEOgH1pKPcTuPYPO0+HcRKf7fSjmB5QzZ7RqXHzOQxLYwVgPHh8WP5gqj4bTzW8xKOw==', -- System@FoodX2024!
    UPPER(REPLACE(NEWID(), '-', '')),
    NEWID(),
    0,
    0,
    1,
    0,
    'System',
    'Account',
    'FoodX Platform'
);

-- Assign SuperAdmin role to system@foodx.com
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES (@SystemId, @SuperAdminRoleId);

PRINT 'Created system@foodx.com with SuperAdmin role';

-- Verify the users were created
SELECT 
    u.Email,
    u.UserName,
    u.EmailConfirmed,
    r.Name as RoleName,
    'Password: ' + CASE 
        WHEN u.Email = 'admin@foodx.com' THEN 'FoodX@Admin2024!'
        WHEN u.Email = 'system@foodx.com' THEN 'System@FoodX2024!'
    END as Password
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email IN ('admin@foodx.com', 'system@foodx.com');
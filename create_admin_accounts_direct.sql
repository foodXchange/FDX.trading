-- Create Admin Accounts Directly in Database
USE FoodXDB;
GO

-- Ensure roles exist
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'SuperAdmin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'SuperAdmin', 'SUPERADMIN', NEWID());
END

-- Clean up any existing admin accounts
DELETE FROM AspNetUserRoles WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email IN ('admin@foodx.com', 'system@foodx.com'));
DELETE FROM AspNetUsers WHERE Email IN ('admin@foodx.com', 'system@foodx.com');

-- Create admin@foodx.com
DECLARE @AdminId NVARCHAR(450) = LOWER(NEWID());
DECLARE @SuperAdminRoleId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetRoles WHERE Name = 'SuperAdmin');

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
    CompanyName,
    IsSuperAdmin,
    CreatedAt,
    UpdatedAt
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
    'FoodX Platform',
    1,
    GETUTCDATE(),
    GETUTCDATE()
);

-- Assign SuperAdmin role
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES (@AdminId, @SuperAdminRoleId);

PRINT 'Created admin@foodx.com account';

-- Create system@foodx.com
DECLARE @SystemId NVARCHAR(450) = LOWER(NEWID());

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
    CompanyName,
    IsSuperAdmin,
    CreatedAt,
    UpdatedAt
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
    'FoodX Platform',
    1,
    GETUTCDATE(),
    GETUTCDATE()
);

-- Assign SuperAdmin role
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES (@SystemId, @SuperAdminRoleId);

PRINT 'Created system@foodx.com account';

-- Verify accounts were created
SELECT 
    u.Email,
    u.FirstName,
    u.LastName,
    u.IsSuperAdmin,
    r.Name as RoleName
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email IN ('admin@foodx.com', 'system@foodx.com');

PRINT '';
PRINT '==============================================';
PRINT 'Admin accounts created successfully!';
PRINT 'Login with:';
PRINT 'Email: admin@foodx.com';
PRINT 'Password: FoodX@Admin2024!';
PRINT '';
PRINT 'Email: system@foodx.com';
PRINT 'Password: System@FoodX2024!';
PRINT '==============================================';
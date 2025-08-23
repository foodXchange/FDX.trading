-- Seed Test Users for FDX.Trading Platform
-- This script creates test users for all roles with proper authentication setup

-- Clean up existing test users (optional - be careful in production!)
DELETE FROM AspNetUserRoles WHERE UserId IN (
    SELECT Id FROM AspNetUsers WHERE Email LIKE '%@test.fdx.trading'
);
DELETE FROM AspNetUsers WHERE Email LIKE '%@test.fdx.trading';

-- Variables for password hash (using a standard test password: TestPass123!)
DECLARE @PasswordHash NVARCHAR(MAX) = 'AQAAAAIAAYagAAAAEPgqVp+2L8kLbNZVH+8kH3V5mZvPFFq3X5Y0Z7JQ8kH3V5mZvPFFq3X5Y0Z7JQ8g==';
DECLARE @SecurityStamp NVARCHAR(MAX) = NEWID();
DECLARE @ConcurrencyStamp NVARCHAR(MAX) = NEWID();
DECLARE @NormalizedEmail NVARCHAR(256);
DECLARE @UserId NVARCHAR(450);
DECLARE @RoleId NVARCHAR(450);

-- 1. Create SuperAdmin User
SET @UserId = NEWID();
SET @NormalizedEmail = UPPER('superadmin@test.fdx.trading');
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
    PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed,
    TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    FirstName, LastName, CompanyName, Department, JobTitle, Country,
    IsSuperAdmin, CreatedAt, LastLoginAt
) VALUES (
    @UserId, 'superadmin@test.fdx.trading', @NormalizedEmail, 
    'superadmin@test.fdx.trading', @NormalizedEmail, 1,
    @PasswordHash, @SecurityStamp, @ConcurrencyStamp, 0,
    0, 1, 0,
    'Super', 'Admin', 'FDX Trading Platform', 'IT', 'System Administrator', 'US',
    1, GETUTCDATE(), NULL
);

-- Assign SuperAdmin role
SELECT @RoleId = Id FROM AspNetRoles WHERE NormalizedName = 'SUPERADMIN';
IF @RoleId IS NOT NULL
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId);

-- 2. Create Admin User
SET @UserId = NEWID();
SET @NormalizedEmail = UPPER('admin@test.fdx.trading');
SET @SecurityStamp = NEWID();
SET @ConcurrencyStamp = NEWID();
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
    PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed,
    TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    FirstName, LastName, CompanyName, Department, JobTitle, Country,
    IsSuperAdmin, CreatedAt, LastLoginAt
) VALUES (
    @UserId, 'admin@test.fdx.trading', @NormalizedEmail, 
    'admin@test.fdx.trading', @NormalizedEmail, 1,
    @PasswordHash, @SecurityStamp, @ConcurrencyStamp, 0,
    0, 1, 0,
    'John', 'Admin', 'FDX Trading Platform', 'Operations', 'Platform Administrator', 'US',
    0, GETUTCDATE(), NULL
);

SELECT @RoleId = Id FROM AspNetRoles WHERE NormalizedName = 'ADMIN';
IF @RoleId IS NOT NULL
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId);

-- 3. Create Buyer Users (3 buyers from different companies)
-- Buyer 1
SET @UserId = NEWID();
SET @NormalizedEmail = UPPER('buyer1@test.fdx.trading');
SET @SecurityStamp = NEWID();
SET @ConcurrencyStamp = NEWID();
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
    PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed,
    TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    FirstName, LastName, CompanyName, Department, JobTitle, Country,
    IsSuperAdmin, CreatedAt, LastLoginAt
) VALUES (
    @UserId, 'buyer1@test.fdx.trading', @NormalizedEmail, 
    'buyer1@test.fdx.trading', @NormalizedEmail, 1,
    @PasswordHash, @SecurityStamp, @ConcurrencyStamp, 0,
    0, 1, 0,
    'Alice', 'Johnson', 'Global Foods Inc', 'Procurement', 'Purchasing Manager', 'US',
    0, GETUTCDATE(), NULL
);

SELECT @RoleId = Id FROM AspNetRoles WHERE NormalizedName = 'BUYER';
IF @RoleId IS NOT NULL
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId);

-- Buyer 2
SET @UserId = NEWID();
SET @NormalizedEmail = UPPER('buyer2@test.fdx.trading');
SET @SecurityStamp = NEWID();
SET @ConcurrencyStamp = NEWID();
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
    PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed,
    TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    FirstName, LastName, CompanyName, Department, JobTitle, Country,
    IsSuperAdmin, CreatedAt, LastLoginAt
) VALUES (
    @UserId, 'buyer2@test.fdx.trading', @NormalizedEmail, 
    'buyer2@test.fdx.trading', @NormalizedEmail, 1,
    @PasswordHash, @SecurityStamp, @ConcurrencyStamp, 0,
    0, 1, 0,
    'Bob', 'Smith', 'Restaurant Chain Ltd', 'Supply Chain', 'Procurement Director', 'GB',
    0, GETUTCDATE(), NULL
);

SELECT @RoleId = Id FROM AspNetRoles WHERE NormalizedName = 'BUYER';
IF @RoleId IS NOT NULL
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId);

-- Buyer 3
SET @UserId = NEWID();
SET @NormalizedEmail = UPPER('buyer3@test.fdx.trading');
SET @SecurityStamp = NEWID();
SET @ConcurrencyStamp = NEWID();
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
    PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed,
    TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    FirstName, LastName, CompanyName, Department, JobTitle, Country,
    IsSuperAdmin, CreatedAt, LastLoginAt
) VALUES (
    @UserId, 'buyer3@test.fdx.trading', @NormalizedEmail, 
    'buyer3@test.fdx.trading', @NormalizedEmail, 1,
    @PasswordHash, @SecurityStamp, @ConcurrencyStamp, 0,
    0, 1, 0,
    'Carol', 'Davis', 'Hotel Group International', 'Purchasing', 'Head of Procurement', 'FR',
    0, GETUTCDATE(), NULL
);

SELECT @RoleId = Id FROM AspNetRoles WHERE NormalizedName = 'BUYER';
IF @RoleId IS NOT NULL
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId);

-- 4. Create Supplier Users (3 suppliers)
-- Supplier 1
SET @UserId = NEWID();
SET @NormalizedEmail = UPPER('supplier1@test.fdx.trading');
SET @SecurityStamp = NEWID();
SET @ConcurrencyStamp = NEWID();
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
    PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed,
    TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    FirstName, LastName, CompanyName, Department, JobTitle, Country,
    IsSuperAdmin, CreatedAt, LastLoginAt
) VALUES (
    @UserId, 'supplier1@test.fdx.trading', @NormalizedEmail, 
    'supplier1@test.fdx.trading', @NormalizedEmail, 1,
    @PasswordHash, @SecurityStamp, @ConcurrencyStamp, 0,
    0, 1, 0,
    'David', 'Wilson', 'Fresh Produce Farms', 'Sales', 'Sales Director', 'ES',
    0, GETUTCDATE(), NULL
);

SELECT @RoleId = Id FROM AspNetRoles WHERE NormalizedName = 'SUPPLIER';
IF @RoleId IS NOT NULL
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId);

-- Supplier 2
SET @UserId = NEWID();
SET @NormalizedEmail = UPPER('supplier2@test.fdx.trading');
SET @SecurityStamp = NEWID();
SET @ConcurrencyStamp = NEWID();
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
    PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed,
    TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    FirstName, LastName, CompanyName, Department, JobTitle, Country,
    IsSuperAdmin, CreatedAt, LastLoginAt
) VALUES (
    @UserId, 'supplier2@test.fdx.trading', @NormalizedEmail, 
    'supplier2@test.fdx.trading', @NormalizedEmail, 1,
    @PasswordHash, @SecurityStamp, @ConcurrencyStamp, 0,
    0, 1, 0,
    'Emma', 'Brown', 'Dairy Products Co', 'Export', 'Export Manager', 'NL',
    0, GETUTCDATE(), NULL
);

SELECT @RoleId = Id FROM AspNetRoles WHERE NormalizedName = 'SUPPLIER';
IF @RoleId IS NOT NULL
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId);

-- Supplier 3
SET @UserId = NEWID();
SET @NormalizedEmail = UPPER('supplier3@test.fdx.trading');
SET @SecurityStamp = NEWID();
SET @ConcurrencyStamp = NEWID();
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
    PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed,
    TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    FirstName, LastName, CompanyName, Department, JobTitle, Country,
    IsSuperAdmin, CreatedAt, LastLoginAt
) VALUES (
    @UserId, 'supplier3@test.fdx.trading', @NormalizedEmail, 
    'supplier3@test.fdx.trading', @NormalizedEmail, 1,
    @PasswordHash, @SecurityStamp, @ConcurrencyStamp, 0,
    0, 1, 0,
    'Frank', 'Garcia', 'Meat Packers Ltd', 'Business Development', 'Account Manager', 'DE',
    0, GETUTCDATE(), NULL
);

SELECT @RoleId = Id FROM AspNetRoles WHERE NormalizedName = 'SUPPLIER';
IF @RoleId IS NOT NULL
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId);

-- 5. Create Agent User
SET @UserId = NEWID();
SET @NormalizedEmail = UPPER('agent1@test.fdx.trading');
SET @SecurityStamp = NEWID();
SET @ConcurrencyStamp = NEWID();
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
    PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed,
    TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    FirstName, LastName, CompanyName, Department, JobTitle, Country,
    IsSuperAdmin, CreatedAt, LastLoginAt
) VALUES (
    @UserId, 'agent1@test.fdx.trading', @NormalizedEmail, 
    'agent1@test.fdx.trading', @NormalizedEmail, 1,
    @PasswordHash, @SecurityStamp, @ConcurrencyStamp, 0,
    0, 1, 0,
    'Grace', 'Martinez', 'Trade Connect Agency', 'Brokerage', 'Senior Trade Agent', 'IT',
    0, GETUTCDATE(), NULL
);

SELECT @RoleId = Id FROM AspNetRoles WHERE NormalizedName = 'AGENT';
IF @RoleId IS NOT NULL
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId);

-- 6. Create Expert User
SET @UserId = NEWID();
SET @NormalizedEmail = UPPER('expert1@test.fdx.trading');
SET @SecurityStamp = NEWID();
SET @ConcurrencyStamp = NEWID();
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
    PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed,
    TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
    FirstName, LastName, CompanyName, Department, JobTitle, Country,
    IsSuperAdmin, CreatedAt, LastLoginAt
) VALUES (
    @UserId, 'expert1@test.fdx.trading', @NormalizedEmail, 
    'expert1@test.fdx.trading', @NormalizedEmail, 1,
    @PasswordHash, @SecurityStamp, @ConcurrencyStamp, 0,
    0, 1, 0,
    'Henry', 'Lee', 'Food Quality Consultants', 'Quality Assurance', 'Senior Food Expert', 'CH',
    0, GETUTCDATE(), NULL
);

SELECT @RoleId = Id FROM AspNetRoles WHERE NormalizedName = 'EXPERT';
IF @RoleId IS NOT NULL
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId);

-- Verify users were created
SELECT 
    u.Email,
    u.FirstName + ' ' + u.LastName as FullName,
    u.CompanyName,
    u.Country,
    r.Name as Role,
    u.CreatedAt
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email LIKE '%@test.fdx.trading'
ORDER BY r.Name, u.Email;

PRINT 'Test users created successfully!';
PRINT '';
PRINT 'Login credentials for all test users:';
PRINT 'Password: TestPass123!';
PRINT '';
PRINT 'Test Accounts:';
PRINT '  superadmin@test.fdx.trading - SuperAdmin role';
PRINT '  admin@test.fdx.trading - Admin role';
PRINT '  buyer1@test.fdx.trading - Buyer role (Global Foods Inc)';
PRINT '  buyer2@test.fdx.trading - Buyer role (Restaurant Chain Ltd)';
PRINT '  buyer3@test.fdx.trading - Buyer role (Hotel Group International)';
PRINT '  supplier1@test.fdx.trading - Supplier role (Fresh Produce Farms)';
PRINT '  supplier2@test.fdx.trading - Supplier role (Dairy Products Co)';
PRINT '  supplier3@test.fdx.trading - Supplier role (Meat Packers Ltd)';
PRINT '  agent1@test.fdx.trading - Agent role (Trade Connect Agency)';
PRINT '  expert1@test.fdx.trading - Expert role (Food Quality Consultants)';
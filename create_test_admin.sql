-- Create a test admin account with a simple password
USE FoodXDB;
GO

-- Delete existing test admin
DELETE FROM AspNetUserRoles WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email = 'test@foodx.com');
DELETE FROM AspNetUsers WHERE Email = 'test@foodx.com';

-- Get SuperAdmin role ID
DECLARE @SuperAdminRoleId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetRoles WHERE Name = 'SuperAdmin');
IF @SuperAdminRoleId IS NULL
BEGIN
    SET @SuperAdminRoleId = NEWID();
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (@SuperAdminRoleId, 'SuperAdmin', 'SUPERADMIN', NEWID());
END

-- Create test account
DECLARE @TestId NVARCHAR(450) = LOWER(NEWID());

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
    AccessFailedCount
)
VALUES (
    @TestId,
    'test@foodx.com',
    'TEST@FOODX.COM',
    'test@foodx.com',
    'TEST@FOODX.COM',
    1,
    NULL, -- No password hash - will need to reset via app
    UPPER(REPLACE(NEWID(), '-', '')),
    NEWID(),
    0,
    0,
    1,
    0
);

-- Assign SuperAdmin role
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES (@TestId, @SuperAdminRoleId);

PRINT 'Test admin account created: test@foodx.com';
PRINT 'You will need to use the "Forgot Password" feature to set a password';

SELECT Email, UserName FROM AspNetUsers WHERE Email = 'test@foodx.com';
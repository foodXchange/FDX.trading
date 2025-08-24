-- Check record counts for existing tables
SELECT 'AspNetUsers' as TableName, COUNT(*) as RecordCount FROM AspNetUsers
UNION ALL
SELECT 'AspNetRoles', COUNT(*) FROM AspNetRoles
UNION ALL
SELECT 'AspNetUserRoles', COUNT(*) FROM AspNetUserRoles
UNION ALL
SELECT '__EFMigrationsHistory', COUNT(*) FROM __EFMigrationsHistory
ORDER BY TableName;

PRINT '------- USERS -------';
-- Show all users with details
SELECT 
    Id,
    Email,
    UserName,
    FirstName,
    LastName,
    Country,
    EmailConfirmed,
    PhoneNumber,
    TwoFactorEnabled,
    LockoutEnabled,
    AccessFailedCount
FROM AspNetUsers
ORDER BY Email;

PRINT '------- ROLES -------';
-- Show all roles
SELECT * FROM AspNetRoles;

PRINT '------- USER-ROLE ASSIGNMENTS -------';
-- Show user-role assignments
SELECT 
    u.Email,
    u.FirstName,
    u.LastName,
    r.Name as RoleName
FROM AspNetUserRoles ur
INNER JOIN AspNetUsers u ON ur.UserId = u.Id
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
ORDER BY r.Name, u.Email;

PRINT '------- MIGRATIONS -------';
-- Show migration history
SELECT * FROM __EFMigrationsHistory
ORDER BY MigrationId;
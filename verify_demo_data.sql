-- Verify Demo Data Creation
USE FoodXDB;
GO

-- Check Invitations
SELECT TOP 10
    InvitationCode,
    Role,
    CompanyName,
    CASE 
        WHEN IsUsed = 1 THEN 'Used'
        WHEN IsRevoked = 1 THEN 'Revoked'
        WHEN ExpiresAt < GETUTCDATE() THEN 'Expired'
        ELSE 'Active'
    END as Status,
    CreatedAt
FROM Invitations 
WHERE InvitationCode LIKE '%-DEMO-%' OR InvitationCode LIKE '%-TEST-%'
ORDER BY Role, InvitationCode;

-- Check Super Admin exists
SELECT TOP 5
    u.Email,
    u.UserName,
    u.FirstName,
    u.LastName,
    ur.RoleId,
    r.Name as RoleName
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email IN ('admin@foodx.com', 'system@foodx.com')
   OR r.Name IN ('Admin', 'SuperAdmin');

-- Check Roles exist
SELECT Id, Name FROM AspNetRoles ORDER BY Name;
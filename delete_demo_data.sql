-- Delete Demo/Test Data from FoodX Trading Platform
-- WARNING: This will permanently delete all test data

BEGIN TRANSACTION;

-- Delete test user roles
DELETE FROM AspNetUserRoles 
WHERE UserId IN (
    SELECT Id FROM AspNetUsers 
    WHERE Email LIKE '%@test.%' 
    OR Email LIKE '%demo%' 
    OR Email LIKE '%test%'
);

-- Delete test user claims
DELETE FROM AspNetUserClaims 
WHERE UserId IN (
    SELECT Id FROM AspNetUsers 
    WHERE Email LIKE '%@test.%' 
    OR Email LIKE '%demo%' 
    OR Email LIKE '%test%'
);

-- Delete test user logins
DELETE FROM AspNetUserLogins 
WHERE UserId IN (
    SELECT Id FROM AspNetUsers 
    WHERE Email LIKE '%@test.%' 
    OR Email LIKE '%demo%' 
    OR Email LIKE '%test%'
);

-- Delete test user tokens
DELETE FROM AspNetUserTokens 
WHERE UserId IN (
    SELECT Id FROM AspNetUsers 
    WHERE Email LIKE '%@test.%' 
    OR Email LIKE '%demo%' 
    OR Email LIKE '%test%'
);

-- Delete test users from AspNetUsers
DELETE FROM AspNetUsers 
WHERE Email LIKE '%@test.%' 
OR Email LIKE '%demo%' 
OR Email LIKE '%test%';

-- Delete test companies (if any)
DELETE FROM Companies 
WHERE Name LIKE '%Test%' 
OR Name LIKE '%Demo%'
OR Name LIKE 'Global Foods Inc'
OR Name LIKE 'Restaurant Chain Ltd'
OR Name LIKE 'Fresh Produce Farms'
OR Name LIKE 'Hotel Group International'
OR Name LIKE 'Dairy Products Co'
OR Name LIKE 'Food Quality Consultants'
OR Name LIKE 'Trade Connect Agency'
OR Name LIKE 'Meat Packers Ltd';

-- Delete test products (if any)
DELETE FROM Products 
WHERE Name LIKE '%Test%' 
OR Name LIKE '%Demo%';

-- Delete test invitations
DELETE FROM Invitations 
WHERE Email LIKE '%@test.%' 
OR Email LIKE '%demo%' 
OR Email LIKE '%test%';

-- Delete test magic links
DELETE FROM MagicLinks 
WHERE Email LIKE '%@test.%' 
OR Email LIKE '%demo%' 
OR Email LIKE '%test%';

-- Show summary of deleted records
PRINT 'Demo data deletion completed';
PRINT 'Deleted test users and related data';

COMMIT TRANSACTION;

-- Verify deletion
SELECT COUNT(*) as RemainingTestUsers FROM AspNetUsers 
WHERE Email LIKE '%@test.%' 
OR Email LIKE '%demo%' 
OR Email LIKE '%test%';
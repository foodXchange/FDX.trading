-- Database Cleanup and Optimization Script
-- This script removes test data and optimizes the database

-- 1. Remove test/demo data from user-related tables
DELETE FROM AspNetUserRoles WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email LIKE '%test%' OR Email LIKE '%demo%');
DELETE FROM AspNetUserClaims WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email LIKE '%test%' OR Email LIKE '%demo%');
DELETE FROM AspNetUserLogins WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email LIKE '%test%' OR Email LIKE '%demo%');
DELETE FROM AspNetUserTokens WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email LIKE '%test%' OR Email LIKE '%demo%');

-- Clean up MagicLinks if table exists
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MagicLinks')
BEGIN
    DELETE FROM MagicLinks WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email LIKE '%test%' OR Email LIKE '%demo%');
    -- Clean up expired magic links (older than 24 hours)
    DELETE FROM MagicLinks WHERE ExpiresAt < DATEADD(HOUR, -24, GETDATE());
END

-- Delete test users
DELETE FROM AspNetUsers WHERE Email LIKE '%test%' OR Email LIKE '%demo%';

-- 2. Update statistics on all tables for better query performance
UPDATE STATISTICS AspNetUsers;
UPDATE STATISTICS AspNetRoles;
UPDATE STATISTICS AspNetUserRoles;

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Companies')
    UPDATE STATISTICS Companies;

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products')
    UPDATE STATISTICS Products;

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Invitations')
    UPDATE STATISTICS Invitations;

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MagicLinks')
    UPDATE STATISTICS MagicLinks;

-- 3. Report on remaining data
PRINT '=== Database Cleanup Summary ===';
SELECT 'Total Users' AS Metric, COUNT(*) AS Count FROM AspNetUsers;
SELECT 'Total Roles' AS Metric, COUNT(*) AS Count FROM AspNetRoles;
SELECT 'Total User-Role Assignments' AS Metric, COUNT(*) AS Count FROM AspNetUserRoles;

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Companies')
    SELECT 'Total Companies' AS Metric, COUNT(*) AS Count FROM Companies;

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products')
    SELECT 'Total Products' AS Metric, COUNT(*) AS Count FROM Products;

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MagicLinks')
    SELECT 'Total Active Magic Links' AS Metric, COUNT(*) AS Count FROM MagicLinks WHERE ExpiresAt > GETDATE();

PRINT 'Database cleanup completed successfully!';
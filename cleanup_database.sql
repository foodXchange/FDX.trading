-- Database Cleanup and Optimization Script
-- This script removes test data and optimizes the database

-- 1. Remove test/demo data from various tables
DELETE FROM LoginAttempts WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email LIKE '%test%' OR Email LIKE '%demo%');
DELETE FROM MagicLinks WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email LIKE '%test%' OR Email LIKE '%demo%');
DELETE FROM AspNetUserRoles WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email LIKE '%test%' OR Email LIKE '%demo%');
DELETE FROM AspNetUserClaims WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email LIKE '%test%' OR Email LIKE '%demo%');
DELETE FROM AspNetUserLogins WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email LIKE '%test%' OR Email LIKE '%demo%');
DELETE FROM AspNetUserTokens WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email LIKE '%test%' OR Email LIKE '%demo%');
DELETE FROM AspNetUsers WHERE Email LIKE '%test%' OR Email LIKE '%demo%';

-- 2. Clean up expired magic links (older than 24 hours)
DELETE FROM MagicLinks WHERE ExpiresAt < DATEADD(HOUR, -24, GETDATE());

-- 3. Clean up old login attempts (older than 30 days)
DELETE FROM LoginAttempts WHERE CreatedAt < DATEADD(DAY, -30, GETDATE());

-- 4. Update statistics on all tables for better query performance
UPDATE STATISTICS AspNetUsers;
UPDATE STATISTICS AspNetRoles;
UPDATE STATISTICS AspNetUserRoles;
UPDATE STATISTICS Companies;
UPDATE STATISTICS Products;
UPDATE STATISTICS Invitations;
UPDATE STATISTICS MagicLinks;
UPDATE STATISTICS LoginAttempts;

-- 5. Report on remaining data
SELECT 'Database Cleanup Summary' AS Report;
SELECT 'Total Users' AS Metric, COUNT(*) AS Count FROM AspNetUsers;
SELECT 'Total Companies' AS Metric, COUNT(*) AS Count FROM Companies;
SELECT 'Total Products' AS Metric, COUNT(*) AS Count FROM Products;
SELECT 'Total Active Magic Links' AS Metric, COUNT(*) AS Count FROM MagicLinks WHERE ExpiresAt > GETDATE();
SELECT 'Total Login Attempts (Last 7 Days)' AS Metric, COUNT(*) AS Count FROM LoginAttempts WHERE CreatedAt > DATEADD(DAY, -7, GETDATE());

PRINT 'Database cleanup completed successfully!';
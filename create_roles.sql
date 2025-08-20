-- Create Roles in AspNetRoles table
USE FoodXDB;
GO

-- Check and insert roles if they don't exist
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'SuperAdmin')
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'SuperAdmin', 'SUPERADMIN', NEWID());

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Admin')
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID());

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Buyer')
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Buyer', 'BUYER', NEWID());

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Supplier')
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Supplier', 'SUPPLIER', NEWID());

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Expert')
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Expert', 'EXPERT', NEWID());

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Agent')
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Agent', 'AGENT', NEWID());

-- Display all roles
SELECT Id, Name FROM AspNetRoles ORDER BY Name;

PRINT 'Roles created/verified successfully!';
-- Create role-specific tables for FoodX Trading Platform
-- Each table links to the main Users table via UserId

-- 1. Create Buyers table
CREATE TABLE [dbo].[Buyers] (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId INT NOT NULL,
    CompanyId INT NULL,
    Department NVARCHAR(100) NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (UserId) REFERENCES [dbo].[Users](Id),
    FOREIGN KEY (CompanyId) REFERENCES [dbo].[Companies](Id)
);

-- 2. Create Suppliers table
CREATE TABLE [dbo].[Suppliers] (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId INT NOT NULL,
    CompanyId INT NULL,
    SupplierType NVARCHAR(50) NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (UserId) REFERENCES [dbo].[Users](Id),
    FOREIGN KEY (CompanyId) REFERENCES [dbo].[Companies](Id)
);

-- 3. Create Experts table
CREATE TABLE [dbo].[Experts] (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId INT NOT NULL,
    Specialization NVARCHAR(200) NULL,
    CertificationNumber NVARCHAR(50) NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (UserId) REFERENCES [dbo].[Users](Id)
);

-- 4. Create Agents table
CREATE TABLE [dbo].[Agents] (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId INT NOT NULL,
    AgencyName NVARCHAR(200) NULL,
    TerritoryRegion NVARCHAR(100) NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (UserId) REFERENCES [dbo].[Users](Id)
);

-- 5. Create SystemAdmins table
CREATE TABLE [dbo].[SystemAdmins] (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId INT NOT NULL,
    AccessLevel NVARCHAR(50) NULL DEFAULT 'Full',
    Department NVARCHAR(100) NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (UserId) REFERENCES [dbo].[Users](Id)
);

-- 6. Create BackOffice table
CREATE TABLE [dbo].[BackOffice] (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId INT NOT NULL,
    Department NVARCHAR(100) NULL,
    ShiftTiming NVARCHAR(50) NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (UserId) REFERENCES [dbo].[Users](Id)
);

-- Insert test users into Users table first (if not exists)
-- We'll insert 30 users total (5 for each role)

-- Insert Buyer users
INSERT INTO [dbo].[Users] (Title, FirstName, LastName, Email, Role, IsActive, CreatedAt, UpdatedAt)
VALUES 
('Mr.', 'John', 'Buyer1', 'buyer1@test.com', 'Buyer', 1, GETUTCDATE(), GETUTCDATE()),
('Ms.', 'Jane', 'Buyer2', 'buyer2@test.com', 'Buyer', 1, GETUTCDATE(), GETUTCDATE()),
('Mr.', 'Bob', 'Buyer3', 'buyer3@test.com', 'Buyer', 1, GETUTCDATE(), GETUTCDATE()),
('Ms.', 'Alice', 'Buyer4', 'buyer4@test.com', 'Buyer', 1, GETUTCDATE(), GETUTCDATE()),
('Mr.', 'Tom', 'Buyer5', 'buyer5@test.com', 'Buyer', 1, GETUTCDATE(), GETUTCDATE());

-- Insert Supplier users
INSERT INTO [dbo].[Users] (Title, FirstName, LastName, Email, Role, IsActive, CreatedAt, UpdatedAt)
VALUES 
('Mr.', 'Sam', 'Supplier1', 'supplier1@test.com', 'Seller', 1, GETUTCDATE(), GETUTCDATE()),
('Ms.', 'Sara', 'Supplier2', 'supplier2@test.com', 'Seller', 1, GETUTCDATE(), GETUTCDATE()),
('Mr.', 'Mike', 'Supplier3', 'supplier3@test.com', 'Seller', 1, GETUTCDATE(), GETUTCDATE()),
('Ms.', 'Emma', 'Supplier4', 'supplier4@test.com', 'Seller', 1, GETUTCDATE(), GETUTCDATE()),
('Mr.', 'David', 'Supplier5', 'supplier5@test.com', 'Seller', 1, GETUTCDATE(), GETUTCDATE());

-- Insert Expert users
INSERT INTO [dbo].[Users] (Title, FirstName, LastName, Email, Role, IsActive, CreatedAt, UpdatedAt)
VALUES 
('Dr.', 'Expert', 'One', 'expert1@test.com', 'Agent', 1, GETUTCDATE(), GETUTCDATE()),
('Dr.', 'Expert', 'Two', 'expert2@test.com', 'Agent', 1, GETUTCDATE(), GETUTCDATE()),
('Dr.', 'Expert', 'Three', 'expert3@test.com', 'Agent', 1, GETUTCDATE(), GETUTCDATE()),
('Dr.', 'Expert', 'Four', 'expert4@test.com', 'Agent', 1, GETUTCDATE(), GETUTCDATE()),
('Dr.', 'Expert', 'Five', 'expert5@test.com', 'Agent', 1, GETUTCDATE(), GETUTCDATE());

-- Insert Agent users
INSERT INTO [dbo].[Users] (Title, FirstName, LastName, Email, Role, IsActive, CreatedAt, UpdatedAt)
VALUES 
('Mr.', 'Agent', 'One', 'agent1@test.com', 'Agent', 1, GETUTCDATE(), GETUTCDATE()),
('Ms.', 'Agent', 'Two', 'agent2@test.com', 'Agent', 1, GETUTCDATE(), GETUTCDATE()),
('Mr.', 'Agent', 'Three', 'agent3@test.com', 'Agent', 1, GETUTCDATE(), GETUTCDATE()),
('Ms.', 'Agent', 'Four', 'agent4@test.com', 'Agent', 1, GETUTCDATE(), GETUTCDATE()),
('Mr.', 'Agent', 'Five', 'agent5@test.com', 'Agent', 1, GETUTCDATE(), GETUTCDATE());

-- Insert Admin users
INSERT INTO [dbo].[Users] (Title, FirstName, LastName, Email, Role, IsActive, CreatedAt, UpdatedAt)
VALUES 
('Mr.', 'Admin', 'One', 'admin1@test.com', 'Admin', 1, GETUTCDATE(), GETUTCDATE()),
('Ms.', 'Admin', 'Two', 'admin2@test.com', 'Admin', 1, GETUTCDATE(), GETUTCDATE()),
('Mr.', 'Admin', 'Three', 'admin3@test.com', 'Admin', 1, GETUTCDATE(), GETUTCDATE()),
('Ms.', 'Admin', 'Four', 'admin4@test.com', 'Admin', 1, GETUTCDATE(), GETUTCDATE()),
('Mr.', 'Admin', 'Five', 'admin5@test.com', 'Admin', 1, GETUTCDATE(), GETUTCDATE());

-- Insert BackOffice users
INSERT INTO [dbo].[Users] (Title, FirstName, LastName, Email, Role, IsActive, CreatedAt, UpdatedAt)
VALUES 
('Mr.', 'BackOffice', 'One', 'backoffice1@test.com', 'Agent', 1, GETUTCDATE(), GETUTCDATE()),
('Ms.', 'BackOffice', 'Two', 'backoffice2@test.com', 'Agent', 1, GETUTCDATE(), GETUTCDATE()),
('Mr.', 'BackOffice', 'Three', 'backoffice3@test.com', 'Agent', 1, GETUTCDATE(), GETUTCDATE()),
('Ms.', 'BackOffice', 'Four', 'backoffice4@test.com', 'Agent', 1, GETUTCDATE(), GETUTCDATE()),
('Mr.', 'BackOffice', 'Five', 'backoffice5@test.com', 'Agent', 1, GETUTCDATE(), GETUTCDATE());

-- Now populate the role-specific tables
-- Get the UserId for each user and insert into respective tables

-- Populate Buyers table
INSERT INTO [dbo].[Buyers] (UserId, CompanyId, Department)
SELECT Id, 1, 'Procurement' FROM [dbo].[Users] WHERE Email LIKE 'buyer%@test.com';

-- Populate Suppliers table
INSERT INTO [dbo].[Suppliers] (UserId, CompanyId, SupplierType)
SELECT Id, 1, 'Food Products' FROM [dbo].[Users] WHERE Email LIKE 'supplier%@test.com';

-- Populate Experts table
INSERT INTO [dbo].[Experts] (UserId, Specialization, CertificationNumber)
SELECT Id, 'Food Quality', 'CERT-' + CAST(ROW_NUMBER() OVER (ORDER BY Id) AS VARCHAR(10)) 
FROM [dbo].[Users] WHERE Email LIKE 'expert%@test.com';

-- Populate Agents table
INSERT INTO [dbo].[Agents] (UserId, AgencyName, TerritoryRegion)
SELECT Id, 'FoodX Agency', 'Middle East' FROM [dbo].[Users] WHERE Email LIKE 'agent%@test.com';

-- Populate SystemAdmins table
INSERT INTO [dbo].[SystemAdmins] (UserId, AccessLevel, Department)
SELECT Id, 'Full', 'IT' FROM [dbo].[Users] WHERE Email LIKE 'admin%@test.com';

-- Populate BackOffice table
INSERT INTO [dbo].[BackOffice] (UserId, Department, ShiftTiming)
SELECT Id, 'Operations', 'Day Shift' FROM [dbo].[Users] WHERE Email LIKE 'backoffice%@test.com';

-- Verify the data
SELECT 'Users' as TableName, COUNT(*) as Count FROM [dbo].[Users]
UNION ALL
SELECT 'Buyers', COUNT(*) FROM [dbo].[Buyers]
UNION ALL
SELECT 'Suppliers', COUNT(*) FROM [dbo].[Suppliers]
UNION ALL
SELECT 'Experts', COUNT(*) FROM [dbo].[Experts]
UNION ALL
SELECT 'Agents', COUNT(*) FROM [dbo].[Agents]
UNION ALL
SELECT 'SystemAdmins', COUNT(*) FROM [dbo].[SystemAdmins]
UNION ALL
SELECT 'BackOffice', COUNT(*) FROM [dbo].[BackOffice];
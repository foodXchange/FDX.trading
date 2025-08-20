-- Create Invitations table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Invitations' AND xtype='U')
BEGIN
    CREATE TABLE Invitations (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        InvitationCode NVARCHAR(100) NOT NULL UNIQUE,
        Email NVARCHAR(256) NOT NULL,
        InvitedByEmail NVARCHAR(256),
        InvitedByName NVARCHAR(50),
        Role NVARCHAR(50) NOT NULL DEFAULT 'Buyer',
        CompanyId INT NULL,
        CompanyName NVARCHAR(200),
        Message NVARCHAR(500),
        ExpiresAt DATETIME2 NOT NULL,
        UsedAt DATETIME2 NULL,
        IsUsed BIT NOT NULL DEFAULT 0,
        IsRevoked BIT NOT NULL DEFAULT 0,
        UsedByUserId NVARCHAR(256),
        CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
        
        CONSTRAINT FK_Invitations_Companies FOREIGN KEY (CompanyId) 
            REFERENCES Companies(Id) ON DELETE SET NULL
    );
    
    CREATE INDEX IX_Invitations_Code ON Invitations(InvitationCode);
    CREATE INDEX IX_Invitations_Email ON Invitations(Email);
    CREATE INDEX IX_Invitations_Status ON Invitations(IsUsed, IsRevoked, ExpiresAt);
    
    PRINT 'Invitations table created successfully';
END
ELSE
BEGIN
    PRINT 'Invitations table already exists';
END

-- Add columns to AspNetUsers for enhanced user management
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'FirstName')
BEGIN
    ALTER TABLE AspNetUsers ADD FirstName NVARCHAR(100) NULL;
    ALTER TABLE AspNetUsers ADD LastName NVARCHAR(100) NULL;
    ALTER TABLE AspNetUsers ADD CompanyName NVARCHAR(200) NULL;
    ALTER TABLE AspNetUsers ADD Department NVARCHAR(100) NULL;
    ALTER TABLE AspNetUsers ADD JobTitle NVARCHAR(100) NULL;
    ALTER TABLE AspNetUsers ADD IsSuperAdmin BIT NOT NULL DEFAULT 0;
    ALTER TABLE AspNetUsers ADD ImpersonatedBy NVARCHAR(450) NULL;
    ALTER TABLE AspNetUsers ADD LastLoginAt DATETIME2 NULL;
    ALTER TABLE AspNetUsers ADD CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME();
    ALTER TABLE AspNetUsers ADD UpdatedAt DATETIME2 DEFAULT SYSUTCDATETIME();
    ALTER TABLE AspNetUsers ADD InvitationCode NVARCHAR(100) NULL;
    ALTER TABLE AspNetUsers ADD InvitationId INT NULL;
    
    PRINT 'Added custom columns to AspNetUsers';
END

-- Create Super Admin Role if not exists
IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'SuperAdmin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'SuperAdmin', 'SUPERADMIN', NEWID());
    
    PRINT 'SuperAdmin role created';
END

-- Create other roles if not exists
IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'Admin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID());
END

IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'Buyer')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Buyer', 'BUYER', NEWID());
END

IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'Supplier')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Supplier', 'SUPPLIER', NEWID());
END

IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'Expert')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Expert', 'EXPERT', NEWID());
END

IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'Agent')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Agent', 'AGENT', NEWID());
END

PRINT 'All roles created/verified';

-- Note: Super admin users will be created via the application's user management
-- to ensure proper password hashing

-- Create sample invitations for testing
INSERT INTO Invitations (InvitationCode, Email, InvitedByEmail, InvitedByName, Role, Message, ExpiresAt)
VALUES 
    (NEWID(), 'newbuyer@test.com', 'admin@foodx.com', 'System Admin', 'Buyer', 
     'Welcome to FoodX B2B Platform! You have been invited as a Buyer.', DATEADD(DAY, 7, SYSUTCDATETIME())),
    (NEWID(), 'newsupplier@test.com', 'admin@foodx.com', 'System Admin', 'Supplier', 
     'Welcome to FoodX B2B Platform! You have been invited as a Supplier.', DATEADD(DAY, 7, SYSUTCDATETIME())),
    (NEWID(), 'newexpert@test.com', 'admin@foodx.com', 'System Admin', 'Expert', 
     'Welcome to FoodX B2B Platform! You have been invited as an Expert.', DATEADD(DAY, 7, SYSUTCDATETIME()));

PRINT 'Sample invitations created';

SELECT 'Invitation System Setup Complete' AS Status;
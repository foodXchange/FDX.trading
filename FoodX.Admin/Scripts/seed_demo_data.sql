-- =====================================================
-- Seed Demo Data for FoodX B2B Platform
-- =====================================================

USE FoodXDB;
GO

-- Clear existing demo data (optional - be careful in production!)
DELETE FROM Invitations WHERE InvitationCode LIKE '%-DEMO-%' OR InvitationCode LIKE '%-TEST-%';
GO

-- =====================================================
-- Create Demo Invitation Codes
-- =====================================================

-- Admin/SuperAdmin Invitations
INSERT INTO Invitations (InvitationCode, Email, Role, CompanyName, Message, ExpiresAt, IsUsed, IsRevoked, CreatedAt, UpdatedAt)
VALUES 
('ADMIN-DEMO-2024', '', 'Admin', 'FoodX Platform', 'Demo admin invitation for testing', DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, GETUTCDATE(), GETUTCDATE()),
('ADMIN-TEST-CODE', '', 'Admin', 'FoodX Platform', 'Test admin invitation', DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, GETUTCDATE(), GETUTCDATE()),
('SUPERADMIN-DEMO-2024', '', 'SuperAdmin', 'FoodX Platform', 'Super admin demo invitation', DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, GETUTCDATE(), GETUTCDATE());

-- Buyer Invitations
INSERT INTO Invitations (InvitationCode, Email, Role, CompanyName, Message, ExpiresAt, IsUsed, IsRevoked, CreatedAt, UpdatedAt)
VALUES 
('BUYER-DEMO-2024', '', 'Buyer', 'Demo Restaurant Group', 'Demo buyer invitation for testing', DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, GETUTCDATE(), GETUTCDATE()),
('BUYER-TEST-CODE', '', 'Buyer', 'Test Food Services', 'Test buyer invitation', DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, GETUTCDATE(), GETUTCDATE()),
('BUYER-HOTEL-DEMO', '', 'Buyer', 'Grand Hotel Chain', 'Hotel buyer demo invitation', DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, GETUTCDATE(), GETUTCDATE()),
('BUYER-RETAIL-DEMO', '', 'Buyer', 'Fresh Market Stores', 'Retail buyer demo invitation', DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, GETUTCDATE(), GETUTCDATE());

-- Supplier Invitations
INSERT INTO Invitations (InvitationCode, Email, Role, CompanyName, Message, ExpiresAt, IsUsed, IsRevoked, CreatedAt, UpdatedAt)
VALUES 
('SUPPLIER-DEMO-2024', '', 'Supplier', 'Fresh Produce Co', 'Demo supplier invitation for testing', DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, GETUTCDATE(), GETUTCDATE()),
('SUPPLIER-TEST-CODE', '', 'Supplier', 'Quality Meats Ltd', 'Test supplier invitation', DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, GETUTCDATE(), GETUTCDATE()),
('SUPPLIER-DAIRY-DEMO', '', 'Supplier', 'Premium Dairy Farm', 'Dairy supplier demo invitation', DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, GETUTCDATE(), GETUTCDATE()),
('SUPPLIER-SEAFOOD-DEMO', '', 'Supplier', 'Ocean Fresh Seafood', 'Seafood supplier demo invitation', DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, GETUTCDATE(), GETUTCDATE());

-- Expert Invitations
INSERT INTO Invitations (InvitationCode, Email, Role, CompanyName, Message, ExpiresAt, IsUsed, IsRevoked, CreatedAt, UpdatedAt)
VALUES 
('EXPERT-DEMO-2024', '', 'Expert', 'Food Safety Consultants', 'Demo expert invitation for testing', DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, GETUTCDATE(), GETUTCDATE()),
('EXPERT-TEST-CODE', '', 'Expert', 'Quality Assurance Labs', 'Test expert invitation', DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, GETUTCDATE(), GETUTCDATE()),
('EXPERT-NUTRITION-DEMO', '', 'Expert', 'Nutrition Advisory Services', 'Nutrition expert demo invitation', DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, GETUTCDATE(), GETUTCDATE());

-- Agent Invitations
INSERT INTO Invitations (InvitationCode, Email, Role, CompanyName, Message, ExpiresAt, IsUsed, IsRevoked, CreatedAt, UpdatedAt)
VALUES 
('AGENT-DEMO-2024', '', 'Agent', 'Global Food Brokers', 'Demo agent invitation for testing', DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, GETUTCDATE(), GETUTCDATE()),
('AGENT-TEST-CODE', '', 'Agent', 'Regional Trade Partners', 'Test agent invitation', DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, GETUTCDATE(), GETUTCDATE()),
('AGENT-INTL-DEMO', '', 'Agent', 'International Food Traders', 'International agent demo invitation', DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, GETUTCDATE(), GETUTCDATE());

-- =====================================================
-- Create Demo Companies
-- =====================================================

-- Check if Companies table exists and insert demo companies
IF OBJECT_ID('dbo.Companies', 'U') IS NOT NULL
BEGIN
    -- Demo Buyer Companies
    IF NOT EXISTS (SELECT 1 FROM Companies WHERE CompanyName = 'Demo Restaurant Group')
    BEGIN
        INSERT INTO Companies (CompanyName, CompanyType, TaxId, Address, City, State, PostalCode, Country, Phone, Email, Website, IsActive, CreatedAt, UpdatedAt)
        VALUES 
        ('Demo Restaurant Group', 'Buyer', 'DEMO-TAX-001', '123 Restaurant Ave', 'New York', 'NY', '10001', 'USA', '+1-555-0100', 'info@demorestaurant.com', 'www.demorestaurant.com', 1, GETUTCDATE(), GETUTCDATE()),
        ('Test Food Services', 'Buyer', 'DEMO-TAX-002', '456 Catering St', 'Los Angeles', 'CA', '90001', 'USA', '+1-555-0101', 'info@testfoodservices.com', 'www.testfoodservices.com', 1, GETUTCDATE(), GETUTCDATE()),
        ('Grand Hotel Chain', 'Buyer', 'DEMO-TAX-003', '789 Hotel Plaza', 'Chicago', 'IL', '60601', 'USA', '+1-555-0102', 'procurement@grandhotel.com', 'www.grandhotel.com', 1, GETUTCDATE(), GETUTCDATE()),
        ('Fresh Market Stores', 'Buyer', 'DEMO-TAX-004', '321 Market Rd', 'Houston', 'TX', '77001', 'USA', '+1-555-0103', 'buying@freshmarket.com', 'www.freshmarket.com', 1, GETUTCDATE(), GETUTCDATE());
    END

    -- Demo Supplier Companies
    IF NOT EXISTS (SELECT 1 FROM Companies WHERE CompanyName = 'Fresh Produce Co')
    BEGIN
        INSERT INTO Companies (CompanyName, CompanyType, TaxId, Address, City, State, PostalCode, Country, Phone, Email, Website, IsActive, CreatedAt, UpdatedAt)
        VALUES 
        ('Fresh Produce Co', 'Supplier', 'DEMO-TAX-005', '100 Farm Road', 'Fresno', 'CA', '93701', 'USA', '+1-555-0200', 'sales@freshproduce.com', 'www.freshproduce.com', 1, GETUTCDATE(), GETUTCDATE()),
        ('Quality Meats Ltd', 'Supplier', 'DEMO-TAX-006', '200 Packing Plant Way', 'Kansas City', 'MO', '64101', 'USA', '+1-555-0201', 'orders@qualitymeats.com', 'www.qualitymeats.com', 1, GETUTCDATE(), GETUTCDATE()),
        ('Premium Dairy Farm', 'Supplier', 'DEMO-TAX-007', '300 Dairy Lane', 'Wisconsin', 'WI', '53201', 'USA', '+1-555-0202', 'sales@premiumdairy.com', 'www.premiumdairy.com', 1, GETUTCDATE(), GETUTCDATE()),
        ('Ocean Fresh Seafood', 'Supplier', 'DEMO-TAX-008', '400 Harbor Blvd', 'Seattle', 'WA', '98101', 'USA', '+1-555-0203', 'orders@oceanfresh.com', 'www.oceanfresh.com', 1, GETUTCDATE(), GETUTCDATE());
    END
END

-- =====================================================
-- Display Created Demo Data
-- =====================================================

PRINT 'Demo invitation codes created successfully!';
PRINT '';
PRINT 'Available Demo Invitation Codes:';
PRINT '================================';
SELECT 
    InvitationCode,
    Role,
    CompanyName,
    CASE 
        WHEN IsUsed = 1 THEN 'Used'
        WHEN IsRevoked = 1 THEN 'Revoked'
        WHEN ExpiresAt < GETUTCDATE() THEN 'Expired'
        ELSE 'Active'
    END as Status
FROM Invitations 
WHERE InvitationCode LIKE '%-DEMO-%' OR InvitationCode LIKE '%-TEST-%'
ORDER BY Role, InvitationCode;

PRINT '';
PRINT 'Demo companies created (if not already existing).';
PRINT 'To use these invitation codes, register at /Account/Register';
PRINT 'Super Admin credentials are in SUPER_ADMIN_CREDENTIALS.md';
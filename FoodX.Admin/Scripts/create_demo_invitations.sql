-- Demo invitation codes for different roles
-- Note: Remove these in production

-- Admin Role Invitations
INSERT INTO Invitations (InvitationCode, Email, Role, InvitedByUserId, CreatedAt, ExpiresAt, IsUsed, IsRevoked, CompanyName)
VALUES 
    ('ADMIN-DEMO-2024', null, 'Admin', 1, GETUTCDATE(), DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, 'FoodX Admin'),
    ('ADMIN-TEST-CODE', null, 'Admin', 1, GETUTCDATE(), DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, 'FoodX Admin');

-- Buyer Role Invitations  
INSERT INTO Invitations (InvitationCode, Email, Role, InvitedByUserId, CreatedAt, ExpiresAt, IsUsed, IsRevoked, CompanyName)
VALUES 
    ('BUYER-DEMO-2024', null, 'Buyer', 1, GETUTCDATE(), DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, 'Demo Buyer Company'),
    ('BUYER-TEST-CODE', null, 'Buyer', 1, GETUTCDATE(), DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, 'Test Buyer Corp');

-- Supplier Role Invitations
INSERT INTO Invitations (InvitationCode, Email, Role, InvitedByUserId, CreatedAt, ExpiresAt, IsUsed, IsRevoked, CompanyName)
VALUES 
    ('SUPPLIER-DEMO-2024', null, 'Supplier', 1, GETUTCDATE(), DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, 'Demo Supplier Inc'),
    ('SUPPLIER-TEST-CODE', null, 'Supplier', 1, GETUTCDATE(), DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, 'Test Supplier LLC');

-- Expert Role Invitations
INSERT INTO Invitations (InvitationCode, Email, Role, InvitedByUserId, CreatedAt, ExpiresAt, IsUsed, IsRevoked, CompanyName)
VALUES 
    ('EXPERT-DEMO-2024', null, 'Expert', 1, GETUTCDATE(), DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, 'Expert Consultants'),
    ('EXPERT-TEST-CODE', null, 'Expert', 1, GETUTCDATE(), DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, 'Food Safety Experts');

-- Agent Role Invitations
INSERT INTO Invitations (InvitationCode, Email, Role, InvitedByUserId, CreatedAt, ExpiresAt, IsUsed, IsRevoked, CompanyName)
VALUES 
    ('AGENT-DEMO-2024', null, 'Agent', 1, GETUTCDATE(), DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, 'FoodX Agents'),
    ('AGENT-TEST-CODE', null, 'Agent', 1, GETUTCDATE(), DATEADD(YEAR, 1, GETUTCDATE()), 0, 0, 'Regional Agents Inc');

-- Display the created invitation codes
SELECT 
    InvitationCode,
    Role,
    CompanyName,
    CreatedAt,
    ExpiresAt,
    CASE WHEN IsUsed = 1 THEN 'Used' ELSE 'Available' END as Status
FROM Invitations 
WHERE InvitationCode LIKE '%-DEMO-%' OR InvitationCode LIKE '%-TEST-%'
ORDER BY Role, InvitationCode;
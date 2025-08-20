-- Get available invitation codes
SELECT TOP 10
    InvitationCode,
    Email,
    Role,
    CompanyName,
    CASE 
        WHEN IsUsed = 1 THEN 'Used'
        WHEN IsRevoked = 1 THEN 'Revoked'
        WHEN ExpiresAt < SYSUTCDATETIME() THEN 'Expired'
        ELSE 'Active'
    END AS Status,
    ExpiresAt,
    Message
FROM Invitations
ORDER BY CreatedAt DESC;

-- Display active invitations only
PRINT '';
PRINT 'ACTIVE INVITATION CODES:';
PRINT '========================';
SELECT 
    InvitationCode as [Code],
    Email,
    Role,
    DATEDIFF(DAY, SYSUTCDATETIME(), ExpiresAt) as [Days Until Expiry]
FROM Invitations
WHERE IsUsed = 0 
    AND IsRevoked = 0 
    AND ExpiresAt > SYSUTCDATETIME()
ORDER BY CreatedAt DESC;
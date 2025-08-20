-- Create Super Admin User
-- Password: FoodX@Admin2024!
-- Note: Password hash is for BCrypt with work factor 10

DECLARE @adminEmail NVARCHAR(256) = 'admin@foodx.com';
DECLARE @systemEmail NVARCHAR(256) = 'system@foodx.com';
DECLARE @passwordHash NVARCHAR(MAX) = 'AQAAAAIAAYagAAAAEGZSHQB9QbFPuRBvLzVfL+mFZJ+3hNJ4ycn7g8VqVJ5p1lVqLmBfq0ZQl3qEXwQzSA=='; -- FoodX@Admin2024!
DECLARE @roleId NVARCHAR(450);
DECLARE @userId1 NVARCHAR(450) = NEWID();
DECLARE @userId2 NVARCHAR(450) = NEWID();

-- Get SuperAdmin role ID
SELECT @roleId = Id FROM AspNetRoles WHERE Name = 'SuperAdmin';

-- Create primary super admin if not exists
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = @adminEmail)
BEGIN
    INSERT INTO AspNetUsers (
        Id, 
        UserName, 
        NormalizedUserName, 
        Email, 
        NormalizedEmail, 
        EmailConfirmed, 
        PasswordHash,
        SecurityStamp,
        ConcurrencyStamp,
        PhoneNumberConfirmed,
        TwoFactorEnabled,
        LockoutEnabled,
        AccessFailedCount,
        FirstName,
        LastName,
        IsSuperAdmin,
        CreatedAt,
        UpdatedAt
    ) VALUES (
        @userId1,
        @adminEmail,
        UPPER(@adminEmail),
        @adminEmail,
        UPPER(@adminEmail),
        1, -- Email confirmed
        @passwordHash,
        NEWID(),
        NEWID(),
        0,
        0,
        1,
        0,
        'System',
        'Administrator',
        1, -- IsSuperAdmin
        SYSUTCDATETIME(),
        SYSUTCDATETIME()
    );
    
    -- Assign SuperAdmin role
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@userId1, @roleId);
    
    PRINT 'Super Admin user created: admin@foodx.com';
    PRINT 'Password: FoodX@Admin2024!';
END
ELSE
BEGIN
    -- Update existing user to be super admin
    UPDATE AspNetUsers 
    SET IsSuperAdmin = 1,
        FirstName = 'System',
        LastName = 'Administrator',
        PasswordHash = @passwordHash,
        EmailConfirmed = 1
    WHERE Email = @adminEmail;
    
    -- Ensure user has SuperAdmin role
    IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles ur 
                   JOIN AspNetUsers u ON ur.UserId = u.Id 
                   WHERE u.Email = @adminEmail AND ur.RoleId = @roleId)
    BEGIN
        INSERT INTO AspNetUserRoles (UserId, RoleId)
        SELECT Id, @roleId FROM AspNetUsers WHERE Email = @adminEmail;
    END
    
    PRINT 'Super Admin user updated: admin@foodx.com';
END

-- Create backup super admin if not exists
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = @systemEmail)
BEGIN
    INSERT INTO AspNetUsers (
        Id, 
        UserName, 
        NormalizedUserName, 
        Email, 
        NormalizedEmail, 
        EmailConfirmed, 
        PasswordHash,
        SecurityStamp,
        ConcurrencyStamp,
        PhoneNumberConfirmed,
        TwoFactorEnabled,
        LockoutEnabled,
        AccessFailedCount,
        FirstName,
        LastName,
        IsSuperAdmin,
        CreatedAt,
        UpdatedAt
    ) VALUES (
        @userId2,
        @systemEmail,
        UPPER(@systemEmail),
        @systemEmail,
        UPPER(@systemEmail),
        1, -- Email confirmed
        @passwordHash,
        NEWID(),
        NEWID(),
        0,
        0,
        1,
        0,
        'System',
        'Owner',
        1, -- IsSuperAdmin
        SYSUTCDATETIME(),
        SYSUTCDATETIME()
    );
    
    -- Assign SuperAdmin role
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@userId2, @roleId);
    
    PRINT 'Backup Super Admin user created: system@foodx.com';
    PRINT 'Password: FoodX@Admin2024!';
END

-- Display super admin users
SELECT 
    u.Email,
    u.FirstName + ' ' + u.LastName as FullName,
    r.Name as Role,
    u.IsSuperAdmin,
    u.EmailConfirmed,
    u.CreatedAt
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.IsSuperAdmin = 1 OR u.Email IN (@adminEmail, @systemEmail)
ORDER BY u.CreatedAt;
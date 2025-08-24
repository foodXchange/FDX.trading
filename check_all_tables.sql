-- Check all tables and their record counts
SELECT 'AspNetUsers' as TableName, COUNT(*) as RecordCount FROM AspNetUsers
UNION ALL
SELECT 'AspNetRoles', COUNT(*) FROM AspNetRoles
UNION ALL
SELECT 'AspNetUserRoles', COUNT(*) FROM AspNetUserRoles
UNION ALL
SELECT 'InvitationCodes', COUNT(*) FROM InvitationCodes
UNION ALL
SELECT 'MagicLinks', COUNT(*) FROM MagicLinks
UNION ALL
SELECT 'Products', COUNT(*) FROM Products
UNION ALL
SELECT 'Suppliers', COUNT(*) FROM Suppliers
UNION ALL
SELECT 'Buyers', COUNT(*) FROM Buyers
UNION ALL
SELECT 'Orders', COUNT(*) FROM Orders
UNION ALL
SELECT 'OrderItems', COUNT(*) FROM OrderItems
UNION ALL
SELECT 'Quotes', COUNT(*) FROM Quotes
UNION ALL
SELECT 'QuoteItems', COUNT(*) FROM QuoteItems
UNION ALL
SELECT 'Projects', COUNT(*) FROM Projects
UNION ALL
SELECT 'Tasks', COUNT(*) FROM Tasks
ORDER BY TableName;

-- Show sample users
SELECT TOP 5 
    Id,
    Email,
    UserName,
    FirstName,
    LastName,
    EmailConfirmed,
    PhoneNumber
FROM AspNetUsers
ORDER BY Email;

-- Show roles
SELECT * FROM AspNetRoles;

-- Show user-role assignments
SELECT 
    u.Email,
    r.Name as RoleName
FROM AspNetUserRoles ur
INNER JOIN AspNetUsers u ON ur.UserId = u.Id
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
ORDER BY u.Email;

-- Show invitation codes
SELECT TOP 10
    Code,
    Email,
    Role,
    IsUsed,
    CreatedAt,
    UsedAt
FROM InvitationCodes
ORDER BY CreatedAt DESC;

-- Show products
SELECT TOP 10 * FROM Products;

-- Show suppliers
SELECT TOP 10 * FROM Suppliers;

-- Show buyers  
SELECT TOP 10 * FROM Buyers;
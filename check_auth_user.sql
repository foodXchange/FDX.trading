-- Check current user and authentication
SELECT 
    SYSTEM_USER AS SystemUser,
    USER_NAME() AS UserName,
    SUSER_NAME() AS ServerUserName,
    SUSER_SNAME() AS ServerLoginName,
    DB_NAME() AS CurrentDatabase;

-- Check if SQL authentication users exist
SELECT 
    name AS LoginName,
    type_desc AS LoginType,
    create_date,
    modify_date,
    default_database_name
FROM sys.sql_logins
WHERE type = 'S';  -- SQL Server authentication
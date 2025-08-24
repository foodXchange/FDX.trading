-- Create SQL Server login for FoodX application
USE master;
GO

-- Drop login if exists
IF EXISTS (SELECT * FROM sys.server_principals WHERE name = 'foodx_app')
BEGIN
    DROP LOGIN foodx_app;
END
GO

-- Create login with strong password
CREATE LOGIN foodx_app WITH PASSWORD = 'FoodX@2024#Secure!DB';
GO

-- Switch to FoodX database
USE FoodX;
GO

-- Drop user if exists
IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'foodx_app')
BEGIN
    DROP USER foodx_app;
END
GO

-- Create user for the login
CREATE USER foodx_app FOR LOGIN foodx_app;
GO

-- Grant necessary permissions
ALTER ROLE db_owner ADD MEMBER foodx_app;
GO

PRINT 'SQL Server authentication setup complete for foodx_app user';
GO
-- Create SQL login for the application
-- Run this in Azure SQL Database as an admin

-- Create a SQL login
CREATE LOGIN foodxapp WITH PASSWORD = 'FoodX@2024!Secure#Trading';

-- Create a user in the fdxdb database
CREATE USER foodxapp FOR LOGIN foodxapp;

-- Grant necessary permissions
ALTER ROLE db_datareader ADD MEMBER foodxapp;
ALTER ROLE db_datawriter ADD MEMBER foodxapp;
ALTER ROLE db_ddladmin ADD MEMBER foodxapp;

-- Grant execute permissions for stored procedures
GRANT EXECUTE TO foodxapp;

-- Grant permissions for Entity Framework migrations
GRANT CREATE TABLE TO foodxapp;
GRANT ALTER ANY SCHEMA TO foodxapp;
GRANT CREATE TYPE TO foodxapp;
GRANT CREATE VIEW TO foodxapp;
GRANT CREATE PROCEDURE TO foodxapp;
GRANT CREATE FUNCTION TO foodxapp;

SELECT 'SQL user foodxapp created successfully' as Result;
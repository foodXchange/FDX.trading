-- Add LastLoginAt column to Users table if it doesn't exist
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
BEGIN
    IF NOT EXISTS (
        SELECT * 
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'Users' 
        AND COLUMN_NAME = 'LastLoginAt'
    )
    BEGIN
        ALTER TABLE Users
        ADD LastLoginAt DATETIME2 NULL;
        PRINT 'LastLoginAt column added to Users table';
    END
    ELSE
    BEGIN
        PRINT 'LastLoginAt column already exists in Users table';
    END
END
ELSE
BEGIN
    PRINT 'Users table does not exist';
END
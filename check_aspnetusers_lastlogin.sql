-- Check if LastLoginAt exists in AspNetUsers table
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetUsers')
BEGIN
    IF NOT EXISTS (
        SELECT * 
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'AspNetUsers' 
        AND COLUMN_NAME = 'LastLoginAt'
    )
    BEGIN
        ALTER TABLE AspNetUsers
        ADD LastLoginAt DATETIME2 NULL;
        PRINT 'LastLoginAt column added to AspNetUsers table';
    END
    ELSE
    BEGIN
        PRINT 'LastLoginAt column already exists in AspNetUsers table';
    END
    
    -- Show current columns
    SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'AspNetUsers'
    ORDER BY ORDINAL_POSITION;
END
ELSE
BEGIN
    PRINT 'AspNetUsers table does not exist';
END
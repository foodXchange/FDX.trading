-- Check if Country column exists in AspNetUsers table
IF NOT EXISTS (
    SELECT * 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'AspNetUsers' 
    AND COLUMN_NAME = 'Country'
)
BEGIN
    -- Add Country column
    ALTER TABLE AspNetUsers 
    ADD Country NVARCHAR(10) NULL;
    
    PRINT 'Country column added to AspNetUsers table';
END
ELSE
BEGIN
    PRINT 'Country column already exists in AspNetUsers table';
END
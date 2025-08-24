-- Add Country column to AspNetUsers table if it doesn't exist
IF NOT EXISTS (
    SELECT * 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'AspNetUsers' 
    AND COLUMN_NAME = 'Country'
)
BEGIN
    ALTER TABLE AspNetUsers
    ADD Country NVARCHAR(100) NULL;
    PRINT 'Country column added successfully';
END
ELSE
BEGIN
    PRINT 'Country column already exists';
END
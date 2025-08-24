-- Add IsActive column to Products table if it doesn't exist
IF NOT EXISTS (
    SELECT * 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Products' 
    AND COLUMN_NAME = 'IsActive'
)
BEGIN
    ALTER TABLE Products
    ADD IsActive BIT NOT NULL DEFAULT 1;
    PRINT 'IsActive column added to Products table';
END
ELSE
BEGIN
    PRINT 'IsActive column already exists in Products table';
END
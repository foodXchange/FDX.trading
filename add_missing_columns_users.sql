-- Add missing columns to Users table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'CompanyName')
BEGIN
    ALTER TABLE Users ADD CompanyName NVARCHAR(200) NULL;
    PRINT 'Added CompanyName column to Users table';
END
ELSE
BEGIN
    PRINT 'CompanyName column already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'LastLoginAt')
BEGIN
    ALTER TABLE Users ADD LastLoginAt DATETIME2 NULL;
    PRINT 'Added LastLoginAt column to Users table';
END
ELSE
BEGIN
    PRINT 'LastLoginAt column already exists';
END

-- Verify columns were added
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length,
    c.is_nullable
FROM sys.columns c
INNER JOIN sys.types t ON c.system_type_id = t.system_type_id
WHERE c.object_id = OBJECT_ID('Users')
    AND c.name IN ('CompanyName', 'LastLoginAt')
ORDER BY c.name;
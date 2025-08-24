-- Add Country column to AspNetUsers table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Country')
BEGIN
    ALTER TABLE AspNetUsers ADD Country NVARCHAR(10) NULL;
    PRINT 'Added Country column to AspNetUsers table';
END
ELSE
BEGIN
    PRINT 'Country column already exists in AspNetUsers table';
END

-- Verify column was added
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length,
    c.is_nullable
FROM sys.columns c
INNER JOIN sys.types t ON c.system_type_id = t.system_type_id
WHERE c.object_id = OBJECT_ID('AspNetUsers')
    AND c.name = 'Country';
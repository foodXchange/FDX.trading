import pyodbc
import sys

# Azure SQL connection parameters
connection_string = (
    "DRIVER={SQL Server};"
    "Server=tcp:fdx-sql-prod.database.windows.net,1433;"
    "Database=fdxdb;"
    "UID=fdxadmin;"
    "PWD=FDX2030!;"
    "Encrypt=yes;"
    "TrustServerCertificate=no;"
    "Connection Timeout=30"
)

sql_commands = """
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
"""

try:
    print("Connecting to database...")
    with pyodbc.connect(connection_string) as conn:
        cursor = conn.cursor()
        
        # Execute the SQL commands
        cursor.execute(sql_commands)
        conn.commit()
        print("Successfully updated database schema")
        
        # Verify column exists
        cursor.execute("""
            SELECT 
                c.name AS ColumnName,
                t.name AS DataType,
                c.max_length,
                c.is_nullable
            FROM sys.columns c
            INNER JOIN sys.types t ON c.system_type_id = t.system_type_id
            WHERE c.object_id = OBJECT_ID('AspNetUsers')
                AND c.name = 'Country'
        """)
        
        result = cursor.fetchone()
        if result:
            print(f"\nVerification - Country column in AspNetUsers table:")
            print(f"  Column: {result.ColumnName}")
            print(f"  Type: {result.DataType}")
            print(f"  Max Length: {result.max_length}")
            print(f"  Nullable: {result.is_nullable}")
        else:
            print("Warning: Country column not found after creation attempt")
            
except Exception as e:
    print(f"Error: {e}")
    sys.exit(1)
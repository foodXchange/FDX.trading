import pyodbc
import sys

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
"""

try:
    print("Connecting to database...")
    with pyodbc.connect(connection_string) as conn:
        cursor = conn.cursor()
        
        # Execute the SQL commands
        for command in sql_commands.split('\nGO\n'):
            if command.strip():
                print(f"Executing: {command[:50]}...")
                cursor.execute(command)
                # Get any messages
                while cursor.messages:
                    for message in cursor.messages:
                        print(message[1])
                    cursor.messages = []
        
        conn.commit()
        print("Successfully added missing columns")
        
        # Verify columns
        cursor.execute("""
            SELECT 
                c.name AS ColumnName,
                t.name AS DataType,
                c.max_length,
                c.is_nullable
            FROM sys.columns c
            INNER JOIN sys.types t ON c.system_type_id = t.system_type_id
            WHERE c.object_id = OBJECT_ID('Users')
                AND c.name IN ('CompanyName', 'LastLoginAt')
            ORDER BY c.name
        """)
        
        print("\nVerification - Columns in Users table:")
        for row in cursor.fetchall():
            print(f"  {row.ColumnName}: {row.DataType} (max_length={row.max_length}, nullable={row.is_nullable})")
            
except Exception as e:
    print(f"Error: {e}")
    sys.exit(1)
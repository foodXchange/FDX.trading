import pyodbc
import struct
import subprocess

# Get Azure AD token
result = subprocess.run(['az', 'account', 'get-access-token', '--resource', 'https://database.windows.net/', '--query', 'accessToken', '-o', 'tsv'], 
                       capture_output=True, text=True, shell=True)
access_token = result.stdout.strip()

# Convert token for connection
token_bytes = bytes(access_token, 'utf-8')
exptoken = b''
for i in token_bytes:
    exptoken += bytes({i})
    exptoken += bytes(1)
tokenstruct = struct.pack("=i", len(exptoken)) + exptoken

# Azure SQL connection parameters
server = 'fdx-sql-prod.database.windows.net'
database = 'fdxdb'
driver = '{ODBC Driver 17 for SQL Server}'
connection_string = f'DRIVER={driver};SERVER={server};DATABASE={database}'

try:
    # Connect using access token
    conn = pyodbc.connect(connection_string, attrs_before={1256: tokenstruct})
    cursor = conn.cursor()
    
    print("Connected to Azure SQL Database")
    print("=" * 60)
    
    # Read SQL script
    with open('create_identity_tables.sql', 'r') as file:
        sql_script = file.read()
    
    # Execute the entire script
    cursor.execute(sql_script)
    conn.commit()
    
    print("Identity tables created successfully!")
    
    # Verify tables exist
    cursor.execute("""
        SELECT TABLE_NAME 
        FROM INFORMATION_SCHEMA.TABLES 
        WHERE TABLE_NAME LIKE 'AspNet%'
        ORDER BY TABLE_NAME
    """)
    
    tables = cursor.fetchall()
    print("\nCreated Identity Tables:")
    print("-" * 30)
    for table in tables:
        print(f"  - {table[0]}")
    
    cursor.close()
    conn.close()
    
except Exception as e:
    print(f"Error: {e}")
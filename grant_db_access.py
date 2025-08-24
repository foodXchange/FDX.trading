import pyodbc
import subprocess
import struct

# Get Azure AD token
result = subprocess.run(
    [r'C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd', 
     'account', 'get-access-token', 
     '--resource', 'https://database.windows.net/', 
     '--query', 'accessToken', 
     '-o', 'tsv'], 
    capture_output=True, 
    text=True, 
    shell=True
)
token = result.stdout.strip()

# Connect using Azure AD token to fdxdb database
SQL_COPT_SS_ACCESS_TOKEN = 1256
token_bytes = bytes(token, 'utf-8')
exptoken = b''
for i in token_bytes:
    exptoken += bytes({i})
    exptoken += bytes(1)
token_struct = struct.pack('=i', len(exptoken)) + exptoken

conn_str = (
    'DRIVER={ODBC Driver 17 for SQL Server};'
    'SERVER=fdx-sql-prod.database.windows.net;'
    'DATABASE=fdxdb;'  # Connect directly to fdxdb
    'Encrypt=yes;'
    'TrustServerCertificate=no;'
    'Connection Timeout=30;'
)

try:
    conn = pyodbc.connect(conn_str, attrs_before={SQL_COPT_SS_ACCESS_TOKEN: token_struct})
    print("Connected to fdxdb with Azure AD!")
    
    cursor = conn.cursor()
    
    # Create user and grant permissions
    cursor.execute("IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'foodxapp') CREATE USER foodxapp FOR LOGIN foodxapp")
    cursor.execute("ALTER ROLE db_owner ADD MEMBER foodxapp")
    conn.commit()
    print("Database access granted to foodxapp!")
    
    cursor.close()
    conn.close()
    
    # Test the password
    print("\nTesting foodxapp connection...")
    test_conn_str = f"DRIVER={{ODBC Driver 17 for SQL Server}};SERVER=fdx-sql-prod.database.windows.net;DATABASE=fdxdb;UID=foodxapp;PWD=FoodX@2024!Secure#Trading;"
    test_conn = pyodbc.connect(test_conn_str)
    print("SUCCESS! foodxapp can connect to fdxdb!")
    
    # Test a simple query
    test_cursor = test_conn.cursor()
    test_cursor.execute("SELECT @@VERSION")
    version = test_cursor.fetchone()[0]
    print(f"Connected successfully. Server: {version[:50]}...")
    
    test_cursor.close()
    test_conn.close()
    
except Exception as e:
    print(f"Error: {e}")
import pyodbc
import subprocess

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

# Connect using Azure AD token
import struct
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
    'DATABASE=master;'
    'Encrypt=yes;'
    'TrustServerCertificate=no;'
    'Connection Timeout=30;'
)

try:
    conn = pyodbc.connect(conn_str, attrs_before={SQL_COPT_SS_ACCESS_TOKEN: token_struct})
    print("Connected with Azure AD as sysadmin!")
    
    cursor = conn.cursor()
    
    # List all logins
    cursor.execute("SELECT name, type_desc, is_disabled FROM sys.sql_logins WHERE type = 'S'")
    logins = cursor.fetchall()
    print("\nSQL Logins available:")
    for login in logins:
        print(f"  - {login[0]} ({login[1]}) - Disabled: {login[2]}")
    
    # Create the login if it doesn't exist
    new_password = 'FoodX@2024!Secure#Trading'
    cursor.execute("SELECT COUNT(*) FROM sys.sql_logins WHERE name = 'foodxapp'")
    exists = cursor.fetchone()[0]
    
    if exists == 0:
        print("\nLogin 'foodxapp' doesn't exist. Creating it...")
        cursor.execute(f"CREATE LOGIN foodxapp WITH PASSWORD = '{new_password}'")
        conn.commit()
        print("Login created successfully!")
    else:
        print("\nResetting password for foodxapp...")
        cursor.execute(f"ALTER LOGIN foodxapp WITH PASSWORD = '{new_password}'")
        conn.commit()
        print("Password reset successfully!")
    
    # Grant access to the database
    print("\nGranting database access...")
    cursor.execute("USE fdxdb")
    cursor.execute("IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'foodxapp') CREATE USER foodxapp FOR LOGIN foodxapp")
    cursor.execute("ALTER ROLE db_owner ADD MEMBER foodxapp")
    conn.commit()
    print("Database access granted!")
    
    cursor.close()
    conn.close()
    
    # Test the new password
    print(f"\nTesting password: {new_password}")
    test_conn_str = f"DRIVER={{ODBC Driver 17 for SQL Server}};SERVER=fdx-sql-prod.database.windows.net;DATABASE=fdxdb;UID=foodxapp;PWD={new_password};"
    test_conn = pyodbc.connect(test_conn_str)
    print("SUCCESS! Password works!")
    test_conn.close()
    
except Exception as e:
    print(f"Error: {e}")
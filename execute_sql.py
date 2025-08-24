import pyodbc
import sys
import subprocess
import struct

# Get access token from Azure CLI
result = subprocess.run([r'C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd', 'account', 'get-access-token', '--resource', 'https://database.windows.net/', '--query', 'accessToken', '-o', 'tsv'], 
                       capture_output=True, text=True, shell=True)
access_token = result.stdout.strip()

# Connection string for Azure SQL
conn_str = (
    'DRIVER={ODBC Driver 17 for SQL Server};'
    'SERVER=fdx-sql-prod.database.windows.net;'
    'DATABASE=fdxdb;'
    'Encrypt=yes;'
    'TrustServerCertificate=no;'
    'Connection Timeout=30;'
)

if len(sys.argv) < 2:
    print("Usage: python execute_sql.py <sql_file>")
    sys.exit(1)

sql_file = sys.argv[1]

try:
    # Read SQL file
    with open(sql_file, 'r') as f:
        sql_script = f.read()
    
    # Connect to database with access token
    SQL_COPT_SS_ACCESS_TOKEN = 1256  # This is the connection attribute for access token
    token_bytes = bytes(access_token, 'utf-8')
    exptoken = b''
    for i in token_bytes:
        exptoken += bytes({i})
        exptoken += bytes(1)
    token_struct = struct.pack('=i', len(exptoken)) + exptoken
    
    conn = pyodbc.connect(conn_str, attrs_before={SQL_COPT_SS_ACCESS_TOKEN: token_struct})
    cursor = conn.cursor()
    
    # Execute SQL
    for statement in sql_script.split('GO'):
        if statement.strip():
            cursor.execute(statement)
            # Print any messages from SQL Server
            while cursor.messages:
                print(cursor.messages.pop(0)[1])
    
    conn.commit()
    print(f"Successfully executed {sql_file}")
    
except Exception as e:
    print(f"Error: {e}")
    sys.exit(1)
finally:
    if 'conn' in locals():
        conn.close()
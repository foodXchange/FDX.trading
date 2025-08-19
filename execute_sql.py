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
    with open('create_role_tables.sql', 'r') as file:
        sql_script = file.read()
    
    # Split by GO statements if any, or execute as separate statements
    statements = sql_script.split('\n\n')
    
    for statement in statements:
        if statement.strip() and not statement.strip().startswith('--'):
            try:
                cursor.execute(statement)
                conn.commit()
                # Check if it's a SELECT statement
                if 'SELECT' in statement.upper() and 'INSERT' not in statement.upper():
                    rows = cursor.fetchall()
                    for row in rows:
                        print(row)
                else:
                    print(f"Executed: {statement[:50]}...")
            except Exception as e:
                print(f"Error executing: {statement[:50]}...")
                print(f"Error: {e}")
    
    print("\n" + "=" * 60)
    print("Script execution completed!")
    
    cursor.close()
    conn.close()
    
except Exception as e:
    print(f"Connection error: {e}")
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
    
    print("\n" + "=" * 60)
    print("CHECKING TABLES IN DATABASE")
    print("=" * 60)
    
    # Check all tables
    cursor.execute("""
        SELECT TABLE_NAME 
        FROM INFORMATION_SCHEMA.TABLES 
        WHERE TABLE_TYPE = 'BASE TABLE' 
        ORDER BY TABLE_NAME
    """)
    
    tables = cursor.fetchall()
    print("\nAll Tables in Database:")
    print("-" * 30)
    for table in tables:
        print(f"  - {table[0]}")
    
    # Check row counts for each new role table
    print("\n" + "=" * 60)
    print("ROW COUNTS FOR ROLE TABLES")
    print("=" * 60)
    
    role_tables = ['Buyers', 'Suppliers', 'Experts', 'Agents', 'SystemAdmins', 'BackOffice']
    
    for table in role_tables:
        try:
            cursor.execute(f"SELECT COUNT(*) FROM [dbo].[{table}]")
            count = cursor.fetchone()[0]
            print(f"{table:15} : {count} rows")
        except:
            print(f"{table:15} : Table not found")
    
    # Check Users table count
    cursor.execute("SELECT COUNT(*) FROM [dbo].[Users]")
    user_count = cursor.fetchone()[0]
    print(f"\nTotal Users: {user_count}")
    
    # Show sample data from Users table
    print("\n" + "=" * 60)
    print("SAMPLE USERS (Last 10)")
    print("=" * 60)
    
    cursor.execute("""
        SELECT TOP 10 Id, Email, Role, IsActive 
        FROM [dbo].[Users] 
        ORDER BY Id DESC
    """)
    
    users = cursor.fetchall()
    print(f"{'ID':<5} {'Email':<25} {'Role':<10} {'Active':<6}")
    print("-" * 50)
    for user in users:
        print(f"{user[0]:<5} {user[1]:<25} {user[2]:<10} {user[3]:<6}")
    
    cursor.close()
    conn.close()
    
except Exception as e:
    print(f"Error: {e}")
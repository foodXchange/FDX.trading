import pyodbc
import sys

# Test different authentication methods
connections = [
    {
        "name": "SQL Auth with new password",
        "conn_str": "Driver={ODBC Driver 17 for SQL Server};Server=fdx-sql-prod.database.windows.net;Database=fdxdb;UID=foodxapp;PWD=FoodX2303;Encrypt=yes;TrustServerCertificate=no;"
    },
    {
        "name": "Azure AD Interactive",
        "conn_str": "Driver={ODBC Driver 17 for SQL Server};Server=fdx-sql-prod.database.windows.net;Database=fdxdb;Authentication=ActiveDirectoryInteractive;Encrypt=yes;TrustServerCertificate=no;"
    },
    {
        "name": "Azure AD Default",
        "conn_str": "Driver={ODBC Driver 17 for SQL Server};Server=fdx-sql-prod.database.windows.net;Database=fdxdb;Authentication=ActiveDirectoryDefault;Encrypt=yes;TrustServerCertificate=no;"
    }
]

for conn_info in connections:
    print(f"\nTrying: {conn_info['name']}")
    try:
        conn = pyodbc.connect(conn_info['conn_str'], timeout=10)
        cursor = conn.cursor()
        cursor.execute("SELECT SYSTEM_USER, @@VERSION")
        row = cursor.fetchone()
        print(f"  SUCCESS! Connected as: {row[0]}")
        
        # Test query
        cursor.execute("SELECT COUNT(*) FROM FoodXBuyers")
        count = cursor.fetchone()[0]
        print(f"  FoodXBuyers count: {count}")
        
        conn.close()
        break
    except Exception as e:
        print(f"  FAILED: {str(e)[:100]}")
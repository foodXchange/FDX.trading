import pyodbc
import sys

connection_string = (
    "DRIVER={SQL Server};"
    "Server=tcp:fdx-sql-prod.database.windows.net,1433;"
    "Database=fdxdb;"
    "Uid=foodxapp;"
    "Pwd=FoodX@2024!Secure#Trading;"
    "Encrypt=yes;"
    "TrustServerCertificate=no;"
    "Connection Timeout=30"
)

try:
    print("Testing SQL connection...")
    with pyodbc.connect(connection_string) as conn:
        cursor = conn.cursor()
        cursor.execute("SELECT @@VERSION")
        result = cursor.fetchone()
        print("Connection successful!")
        print(f"SQL Server version: {result[0][:50]}...")
except Exception as e:
    print(f"Connection failed: {e}")
    sys.exit(1)
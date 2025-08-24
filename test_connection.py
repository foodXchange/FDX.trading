import pyodbc

password = r'tw5JeRJ5nWvYbWgMvzh@@!@fJYS#QHmT'
conn_str = f'DRIVER={{ODBC Driver 17 for SQL Server}};SERVER=fdx-sql-prod.database.windows.net;DATABASE=fdxdb;UID=foodxapp;PWD={password};'

print(f"Testing connection with password: {password[:10]}...")
try:
    conn = pyodbc.connect(conn_str)
    print("SUCCESS! Connected to database")
    cursor = conn.cursor()
    cursor.execute("SELECT @@VERSION")
    row = cursor.fetchone()
    print(f"Server version: {row[0][:50]}...")
    conn.close()
except Exception as e:
    print(f"FAILED: {e}")
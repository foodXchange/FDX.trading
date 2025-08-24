"""
Execute SQL scripts using saved credentials
No authentication prompts required
"""
import pyodbc
import sys
from db_connection import get_connection

if len(sys.argv) < 2:
    print("Usage: python execute_sql_with_auth.py <sql_file>")
    sys.exit(1)

sql_file = sys.argv[1]

try:
    # Read SQL file
    with open(sql_file, 'r') as f:
        sql_script = f.read()
    
    # Connect to database using saved credentials
    conn = get_connection()
    cursor = conn.cursor()
    
    # Execute SQL statements
    for statement in sql_script.split('GO'):
        if statement.strip():
            try:
                cursor.execute(statement)
                # Print any messages from SQL Server
                while cursor.messages:
                    print(cursor.messages.pop(0)[1])
            except pyodbc.ProgrammingError as e:
                # Handle statements that don't return results
                if "No results" not in str(e):
                    raise
    
    conn.commit()
    print(f"[SUCCESS] Executed {sql_file}")
    
    # If SELECT statement, show results
    if 'SELECT' in sql_script.upper() and not 'INSERT' in sql_script.upper():
        cursor.execute(sql_script.replace('GO', ''))
        rows = cursor.fetchall()
        if rows:
            # Print column headers
            columns = [column[0] for column in cursor.description]
            print("\nResults:")
            print("-" * 80)
            print(" | ".join(columns))
            print("-" * 80)
            for row in rows[:10]:  # Show first 10 rows
                print(" | ".join(str(val) for val in row))
            if len(rows) > 10:
                print(f"... and {len(rows) - 10} more rows")
    
except Exception as e:
    print(f"[ERROR] {e}")
    sys.exit(1)
finally:
    if 'conn' in locals():
        conn.close()
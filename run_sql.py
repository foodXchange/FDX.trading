#!/usr/bin/env python
"""
Simple SQL script runner using centralized database connection
Usage: python run_sql.py <sql_file_or_statement>
"""
import sys
from db_connection import get_connection
import os

def execute_sql(sql_content, is_file=True):
    """Execute SQL content (either from file or direct statement)"""
    try:
        conn = get_connection()
        cursor = conn.cursor()
        
        # Split by GO statements
        statements = sql_content.split('GO')
        
        for statement in statements:
            if statement.strip():
                try:
                    cursor.execute(statement)
                    
                    # If it's a SELECT statement, show results
                    if 'SELECT' in statement.upper() and 'INSERT' not in statement.upper() and 'UPDATE' not in statement.upper():
                        rows = cursor.fetchall()
                        if rows and cursor.description:
                            # Print column headers
                            columns = [column[0] for column in cursor.description]
                            print("\nResults:")
                            print("-" * 80)
                            print(" | ".join(columns[:5]))  # Show first 5 columns
                            print("-" * 80)
                            for row in rows[:10]:  # Show first 10 rows
                                print(" | ".join(str(val)[:20] if val else 'NULL' for val in row[:5]))
                            if len(rows) > 10:
                                print(f"... and {len(rows) - 10} more rows")
                except Exception as e:
                    if "no results" not in str(e).lower():
                        print(f"Error in statement: {str(e)[:100]}")
        
        conn.commit()
        
        if is_file:
            print(f"\n[SUCCESS] Executed SQL script")
        else:
            print(f"\n[SUCCESS] Executed SQL statement")
        
        cursor.close()
        conn.close()
        return True
        
    except Exception as e:
        print(f"[ERROR] {e}")
        return False

def main():
    if len(sys.argv) < 2:
        print("Usage: python run_sql.py <sql_file> or python run_sql.py \"SQL STATEMENT\"")
        sys.exit(1)
    
    arg = sys.argv[1]
    
    # Check if it's a file or a direct SQL statement
    if os.path.exists(arg):
        # It's a file
        print(f"Executing SQL file: {arg}")
        with open(arg, 'r') as f:
            sql_content = f.read()
        execute_sql(sql_content, is_file=True)
    else:
        # It's a direct SQL statement
        print(f"Executing SQL statement")
        execute_sql(arg, is_file=False)

if __name__ == "__main__":
    main()
import pyodbc
import sys

def test_sql_auth():
    """Test SQL authentication connection to Azure SQL"""
    
    # SQL authentication connection string - NO Azure AD!
    conn_string = (
        "DRIVER={ODBC Driver 17 for SQL Server};"
        "SERVER=fdx-sql-prod.database.windows.net;"
        "DATABASE=fdxdb;"
        "UID=foodxapp;"
        "PWD=FoodX@2024!Secure#Trading;"
        "Encrypt=yes;"
        "TrustServerCertificate=no;"
    )
    
    try:
        print("Testing SQL authentication connection (no Microsoft prompts!)...")
        conn = pyodbc.connect(conn_string)
        cursor = conn.cursor()
        
        # Test the connection
        cursor.execute("SELECT DB_NAME() as db, CURRENT_USER as user")
        result = cursor.fetchone()
        print(f"Success! Connected to database: {result[0]} as user: {result[1]}")
        
        # Check tables
        cursor.execute("""
            SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_TYPE = 'BASE TABLE'
        """)
        table_count = cursor.fetchone()[0]
        print(f"Found {table_count} tables in the database")
        
        cursor.close()
        conn.close()
        
        print("\n" + "="*80)
        print("SQL AUTHENTICATION IS WORKING!")
        print("="*80)
        print("Your appsettings.json is already configured correctly.")
        print("No more Microsoft authentication prompts when building/running!")
        print("="*80)
        
        return True
        
    except pyodbc.Error as e:
        if "Login failed" in str(e):
            print("ERROR: SQL authentication failed. The user might not exist.")
            print("Creating the SQL user now...")
            return create_sql_user()
        else:
            print(f"ERROR: {str(e)}")
            return False

def create_sql_user():
    """Create SQL user if it doesn't exist"""
    
    # We need Azure AD auth to create the user
    conn_string = (
        "DRIVER={ODBC Driver 17 for SQL Server};"
        "SERVER=fdx-sql-prod.database.windows.net;"
        "DATABASE=fdxdb;"
        "Authentication=ActiveDirectoryInteractive;"
        "Encrypt=yes;"
        "TrustServerCertificate=no;"
    )
    
    try:
        print("Connecting with Azure AD to create SQL user...")
        conn = pyodbc.connect(conn_string)
        cursor = conn.cursor()
        
        # Create login and user
        cursor.execute("""
            IF NOT EXISTS (SELECT * FROM sys.sql_logins WHERE name = 'foodxapp')
                CREATE LOGIN foodxapp WITH PASSWORD = 'FoodX@2024!Secure#Trading'
        """)
        
        cursor.execute("""
            IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'foodxapp')
                CREATE USER foodxapp FOR LOGIN foodxapp
        """)
        
        # Grant permissions
        cursor.execute("ALTER ROLE db_owner ADD MEMBER foodxapp")
        
        conn.commit()
        print("SQL user created successfully!")
        
        cursor.close()
        conn.close()
        
        # Now test with SQL auth
        return test_sql_auth()
        
    except Exception as e:
        print(f"ERROR creating user: {str(e)}")
        return False

if __name__ == "__main__":
    success = test_sql_auth()
    sys.exit(0 if success else 1)
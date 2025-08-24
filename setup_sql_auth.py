import pyodbc
import sys

def create_sql_user():
    """Create SQL authentication user for the application"""
    
    # Connection string using Windows Authentication (for admin access)
    conn_string = (
        "DRIVER={ODBC Driver 17 for SQL Server};"
        "SERVER=localhost;"
        "DATABASE=master;"
        "Trusted_Connection=yes;"
    )
    
    try:
        print("Connecting to Local SQL Server...")
        conn = pyodbc.connect(conn_string)
        cursor = conn.cursor()
        
        print("Creating SQL login and user...")
        
        # Check if login exists and drop it
        cursor.execute("""
            IF EXISTS (SELECT * FROM sys.server_principals WHERE name = 'foodxapp')
                DROP LOGIN foodxapp
        """)
        
        # Create login
        cursor.execute("""
            CREATE LOGIN foodxapp WITH PASSWORD = 'FoodX@2024!Secure#Trading'
        """)
        print("✓ Login created")
        
        # Switch to FoodX database
        cursor.execute("USE FoodX")
        
        # Check if user exists and drop it
        cursor.execute("""
            IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'foodxapp')
                DROP USER foodxapp
        """)
        
        # Create user
        cursor.execute("""
            CREATE USER foodxapp FOR LOGIN foodxapp
        """)
        print("✓ User created")
        
        # Grant permissions
        permissions = [
            "ALTER ROLE db_datareader ADD MEMBER foodxapp",
            "ALTER ROLE db_datawriter ADD MEMBER foodxapp", 
            "ALTER ROLE db_ddladmin ADD MEMBER foodxapp",
            "GRANT EXECUTE TO foodxapp",
            "GRANT CREATE TABLE TO foodxapp",
            "GRANT ALTER ANY SCHEMA TO foodxapp",
            "GRANT CREATE TYPE TO foodxapp",
            "GRANT CREATE VIEW TO foodxapp",
            "GRANT CREATE PROCEDURE TO foodxapp",
            "GRANT CREATE FUNCTION TO foodxapp"
        ]
        
        for perm in permissions:
            cursor.execute(perm)
        
        print("✓ Permissions granted")
        
        conn.commit()
        
        # Test the new connection
        print("\nTesting SQL authentication...")
        test_conn_string = (
            "DRIVER={ODBC Driver 17 for SQL Server};"
            "SERVER=localhost;"
            "DATABASE=FoodX;"
            "UID=foodxapp;"
            "PWD=FoodX@2024!Secure#Trading;"
            "TrustServerCertificate=yes;"
        )
        
        test_conn = pyodbc.connect(test_conn_string)
        test_cursor = test_conn.cursor()
        test_cursor.execute("SELECT CURRENT_USER")
        user = test_cursor.fetchone()[0]
        print(f"✓ Successfully connected as: {user}")
        test_conn.close()
        
        print("\n✅ SQL authentication setup complete!")
        print("\n" + "="*80)
        print("UPDATE YOUR appsettings.json WITH THIS CONNECTION STRING:")
        print("="*80)
        print('"DefaultConnection": "Server=localhost;Database=FoodX;User Id=foodxapp;Password=FoodX@2024!Secure#Trading;TrustServerCertificate=true;MultipleActiveResultSets=true"')
        print("="*80)
        print("\nNo more Microsoft authentication prompts!")
        
        cursor.close()
        conn.close()
        return True
        
    except Exception as e:
        print(f"❌ Error: {str(e)}")
        return False

if __name__ == "__main__":
    success = create_sql_user()
    sys.exit(0 if success else 1)
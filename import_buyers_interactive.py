import pyodbc

def import_buyers():
    """Import buyers using Azure AD Interactive authentication."""
    
    # Connection string for Azure AD Interactive - this will pop up login window
    conn_str = (
        'DRIVER={ODBC Driver 17 for SQL Server};'
        'SERVER=fdx-sql-prod.database.windows.net;'
        'DATABASE=fdxdb;'
        'Authentication=ActiveDirectoryInteractive;'
        'Encrypt=yes;'
        'TrustServerCertificate=no;'
    )
    
    print("Attempting to connect with Azure AD Interactive authentication...")
    print("Please log in with foodz-x@hotmail.com in the popup window...")
    
    try:
        conn = pyodbc.connect(conn_str)
        cursor = conn.cursor()
        
        print("\n[SUCCESS] Connected successfully!")
        
        # Check current status
        cursor.execute("SELECT SYSTEM_USER")
        user = cursor.fetchone()[0]
        print(f"Connected as: {user}")
        
        cursor.execute("SELECT COUNT(*) FROM FoodXBuyers")
        total_buyers = cursor.fetchone()[0]
        print(f"\nTotal buyers in FoodXBuyers: {total_buyers}")
        
        cursor.execute("SELECT COUNT(*) FROM Companies WHERE CompanyType = 'Buyer'")
        existing = cursor.fetchone()[0]
        print(f"Current buyers in Companies: {existing}")
        
        to_import = total_buyers - existing
        if to_import <= 0:
            print("\nAll buyers already imported!")
            return
            
        print(f"Buyers to import: {to_import}")
        
        # Import buyers with duplicate handling
        print("\nStarting import...")
        cursor.execute("""
            WITH UniqueBuyers AS (
                SELECT 
                    Company,
                    Type,
                    Region,
                    Website,
                    ProcurementEmail,
                    ROW_NUMBER() OVER (PARTITION BY Company ORDER BY Id) as rn
                FROM FoodXBuyers
                WHERE Company IS NOT NULL 
                AND Company != ''
                AND Company != 'NULL'
            )
            INSERT INTO Companies (Name, CompanyType, BuyerCategory, Country, Website, MainEmail)
            SELECT 
                Company AS Name,
                'Buyer' AS CompanyType,
                Type AS BuyerCategory,
                Region AS Country,
                Website,
                ProcurementEmail AS MainEmail
            FROM UniqueBuyers
            WHERE rn = 1
            AND Company NOT IN (SELECT Name FROM Companies WHERE Name IS NOT NULL)
        """)
        
        imported = cursor.rowcount
        conn.commit()
        print(f"[SUCCESS] Successfully imported {imported} new buyer companies!")
        
        # Final verification
        cursor.execute("SELECT COUNT(*) FROM Companies WHERE CompanyType = 'Buyer'")
        total = cursor.fetchone()[0]
        print(f"\nTotal buyer companies now: {total}")
        
        # Show breakdown
        cursor.execute("""
            SELECT TOP 10 
                ISNULL(BuyerCategory, 'Not Specified') as Category, 
                COUNT(*) as Count
            FROM Companies 
            WHERE CompanyType = 'Buyer'
            GROUP BY BuyerCategory
            ORDER BY Count DESC
        """)
        
        print("\nTop Buyer Categories:")
        print("-" * 40)
        for row in cursor.fetchall():
            print(f"  {row[0]:<25} {row[1]:>5}")
        
        conn.close()
        print("\n[COMPLETE] Import completed successfully!")
        
    except Exception as e:
        print(f"\n[ERROR] Error: {e}")
        if "Login failed" in str(e):
            print("\nPlease make sure to:")
            print("1. Use the email: foodz-x@hotmail.com")
            print("2. Your account has access to the database")
            print("3. Azure AD authentication is enabled on the SQL Server")

if __name__ == "__main__":
    import_buyers()
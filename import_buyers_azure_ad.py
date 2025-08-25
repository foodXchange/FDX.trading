import pyodbc
import subprocess
import struct

def get_access_token():
    """Get Azure AD access token for database authentication"""
    result = subprocess.run(
        [r'C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd', 'account', 'get-access-token', '--resource', 'https://database.windows.net/', '--query', 'accessToken', '-o', 'tsv'], 
        capture_output=True, 
        text=True,
        shell=True
    )
    return result.stdout.strip()

def import_buyers():
    """Import buyers from FoodXBuyers to Companies table using Azure AD auth."""
    
    # Get access token
    access_token = get_access_token()
    token_bytes = bytes(access_token, 'UTF-8')
    exptoken = b''
    for i in token_bytes:
        exptoken += bytes({i})
        exptoken += bytes(1)
    token_struct = struct.pack("=i", len(exptoken)) + exptoken
    
    # Connection string for Azure AD
    conn_str = (
        'DRIVER={ODBC Driver 17 for SQL Server};'
        'SERVER=fdx-sql-prod.database.windows.net;'
        'DATABASE=fdxdb;'
        'Encrypt=yes;'
        'TrustServerCertificate=no;'
    )
    
    try:
        conn = pyodbc.connect(conn_str, attrs_before={1256: token_struct})
        cursor = conn.cursor()
        
        print("Connected successfully with Azure AD!")
        
        # Check current status
        cursor.execute("SELECT COUNT(*) FROM FoodXBuyers")
        total_buyers = cursor.fetchone()[0]
        print(f"Total buyers in FoodXBuyers: {total_buyers}")
        
        cursor.execute("SELECT COUNT(*) FROM Companies WHERE CompanyType = 'Buyer'")
        existing = cursor.fetchone()[0]
        print(f"Current buyers in Companies: {existing}")
        
        # Import buyers with duplicate handling
        print("\nImporting buyers...")
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
        print(f"Successfully imported {imported} new buyer companies!")
        
        # Final verification
        cursor.execute("SELECT COUNT(*) FROM Companies WHERE CompanyType = 'Buyer'")
        total = cursor.fetchone()[0]
        print(f"\nTotal buyer companies now: {total}")
        
        # Show breakdown
        cursor.execute("""
            SELECT TOP 10 BuyerCategory, COUNT(*) as Count
            FROM Companies 
            WHERE CompanyType = 'Buyer'
            GROUP BY BuyerCategory
            ORDER BY Count DESC
        """)
        
        print("\nTop Buyer Categories:")
        for row in cursor.fetchall():
            category = row[0] or 'Unknown'
            print(f"  {category}: {row[1]}")
        
        conn.close()
        print("\nImport completed successfully!")
        
    except Exception as e:
        print(f"Error: {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    import_buyers()
import pyodbc

def import_buyers():
    """Import buyers from FoodXBuyers to Companies table."""
    # Direct connection string
    conn_str = "Driver={ODBC Driver 17 for SQL Server};Server=fdx-sql-prod.database.windows.net;Database=fdxdb;UID=foodxapp;PWD=FoodX2303;Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;"
    
    try:
        conn = pyodbc.connect(conn_str)
        cursor = conn.cursor()
        
        # First check how many we need to import
        cursor.execute("""
            SELECT COUNT(*) 
            FROM FoodXBuyers 
            WHERE Company IS NOT NULL 
            AND Company != ''
            AND Company NOT IN (SELECT Name FROM Companies)
        """)
        total_to_import = cursor.fetchone()[0]
        print(f"Total buyers to import: {total_to_import}")
        
        if total_to_import == 0:
            print("No new buyers to import!")
            return
        
        # Import all at once
        cursor.execute("""
            INSERT INTO Companies (Name, CompanyType, BuyerCategory, Country, Website, MainEmail)
            SELECT DISTINCT
                Company AS Name,
                'Buyer' AS CompanyType,
                Type AS BuyerCategory,
                Region AS Country,
                Website,
                ProcurementEmail AS MainEmail
            FROM FoodXBuyers
            WHERE Company IS NOT NULL 
            AND Company != ''
            AND Company NOT IN (SELECT Name FROM Companies)
        """)
        
        imported = cursor.rowcount
        conn.commit()
        print(f"Successfully imported {imported} buyer companies")
        
        # Final verification
        cursor.execute("SELECT COUNT(*) FROM Companies WHERE CompanyType = 'Buyer'")
        total_buyers = cursor.fetchone()[0]
        print(f"\nTotal buyer companies in database: {total_buyers}")
        
        # Show breakdown by category
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
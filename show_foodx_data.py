import pyodbc
import os

def get_connection():
    """Get database connection using Windows Authentication"""
    conn_str = (
        "Driver={ODBC Driver 17 for SQL Server};"
        "Server=tcp:fdx-sql-prod.database.windows.net,1433;"
        "Database=fdxdb;"
        "Authentication=ActiveDirectoryInteractive;"
        "Encrypt=yes;"
        "TrustServerCertificate=no;"
        "Connection Timeout=30;"
    )
    return pyodbc.connect(conn_str)

def main():
    try:
        conn = get_connection()
        cursor = conn.cursor()
        
        print("=" * 80)
        print("FOODX DATABASE CONTENT REPORT")
        print("=" * 80)
        
        # Get counts
        print("\nTABLE RECORD COUNTS:")
        print("-" * 40)
        
        tables = [
            'FoodXBuyers',
            'FoodXSuppliers',
            'Exhibitors',
            'Exhibitions',
            'FoodXProducts',
            'Orders',
            'OrderItems'
        ]
        
        for table in tables:
            cursor.execute(f"SELECT COUNT(*) FROM {table}")
            count = cursor.fetchone()[0]
            print(f"  {table:20} : {count:8,} records")
        
        # Sample buyers
        print("\nSAMPLE BUYERS:")
        print("-" * 40)
        cursor.execute("""
            SELECT TOP 5 
                Company,
                Type,
                Region,
                Markets
            FROM FoodXBuyers
            ORDER BY Id
        """)
        
        buyers = cursor.fetchall()
        if buyers:
            for buyer in buyers:
                print(f"  - {buyer[0][:40]:40} | {(buyer[1] or '')[:20]:20} | {(buyer[2] or '')[:15]}")
        else:
            print("  No buyers found")
        
        # Sample suppliers
        print("\nSAMPLE SUPPLIERS:")
        print("-" * 40)
        cursor.execute("""
            SELECT TOP 5
                SupplierName,
                Country,
                ProductCategory
            FROM FoodXSuppliers
            ORDER BY Id
        """)
        
        suppliers = cursor.fetchall()
        if suppliers:
            for supplier in suppliers:
                print(f"  - {supplier[0][:40]:40} | {(supplier[1] or '')[:20]:20} | {(supplier[2] or '')[:30]}")
        else:
            print("  No suppliers found")
        
        # Sample exhibitors
        print("\nSAMPLE EXHIBITORS:")
        print("-" * 40)
        cursor.execute("""
            SELECT TOP 5
                e.CompanyName,
                e.Country,
                ex.Name as Exhibition
            FROM Exhibitors e
            LEFT JOIN Exhibitions ex ON e.ExhibitionId = ex.Id
            ORDER BY e.Id
        """)
        
        exhibitors = cursor.fetchall()
        if exhibitors:
            for exhibitor in exhibitors:
                print(f"  - {exhibitor[0][:40]:40} | {(exhibitor[1] or '')[:20]:20} | {(exhibitor[2] or '')[:30]}")
        else:
            print("  No exhibitors found")
        
        # Statistics
        print("\nSTATISTICS:")
        print("-" * 40)
        
        # Buyers with emails
        cursor.execute("""
            SELECT COUNT(*) FROM FoodXBuyers 
            WHERE ProcurementEmail IS NOT NULL AND ProcurementEmail != ''
        """)
        buyers_with_email = cursor.fetchone()[0]
        
        # Suppliers by country
        cursor.execute("""
            SELECT TOP 5 Country, COUNT(*) as cnt
            FROM FoodXSuppliers
            WHERE Country IS NOT NULL AND Country != ''
            GROUP BY Country
            ORDER BY cnt DESC
        """)
        top_countries = cursor.fetchall()
        
        # Total unique exhibitions
        cursor.execute("SELECT COUNT(DISTINCT Name) FROM Exhibitions")
        unique_exhibitions = cursor.fetchone()[0]
        
        print(f"  Buyers with procurement email: {buyers_with_email:,}")
        print(f"  Unique exhibitions: {unique_exhibitions}")
        
        if top_countries:
            print("\n  Top supplier countries:")
            for country, count in top_countries:
                print(f"    - {country:30} : {count:6,} suppliers")
        
        print("\n" + "=" * 80)
        print("[SUCCESS] Data verification complete!")
        print("=" * 80)
        
        cursor.close()
        conn.close()
        
    except Exception as e:
        print(f"[ERROR] {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    main()
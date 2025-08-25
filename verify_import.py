import pyodbc

# Connect with Azure AD Interactive
conn_str = (
    'DRIVER={ODBC Driver 17 for SQL Server};'
    'SERVER=fdx-sql-prod.database.windows.net;'
    'DATABASE=fdxdb;'
    'Authentication=ActiveDirectoryInteractive;'
)

try:
    conn = pyodbc.connect(conn_str)
    cursor = conn.cursor()
    
    print("IMPORT VERIFICATION REPORT")
    print("=" * 50)
    
    # Total companies by type
    cursor.execute("SELECT CompanyType, COUNT(*) as Total FROM Companies GROUP BY CompanyType ORDER BY Total DESC")
    print("\nCompanies by Type:")
    for row in cursor.fetchall():
        comp_type = row[0] or 'Not Specified'
        print(f"  {comp_type}: {row[1]}")
    
    # Total unique companies
    cursor.execute("SELECT COUNT(DISTINCT Name) FROM Companies")
    total = cursor.fetchone()[0]
    print(f"\nTotal unique companies: {total}")
    
    # Sample of imported buyers
    cursor.execute("SELECT TOP 10 Name, BuyerCategory, Country FROM Companies WHERE CompanyType = 'Buyer' ORDER BY Name")
    print("\nSample of imported buyers:")
    print("-" * 50)
    for row in cursor.fetchall():
        name = row[0][:30] if row[0] else 'Unknown'
        category = row[1] or 'Not Specified'
        country = row[2] or 'Not Specified'
        print(f"  {name:<30} | {category:<20} | {country}")
    
    # Check for any issues
    cursor.execute("SELECT COUNT(*) FROM Companies WHERE Name IS NULL OR Name = ''")
    empty_names = cursor.fetchone()[0]
    if empty_names > 0:
        print(f"\n[WARNING] {empty_names} companies with empty names found")
    
    cursor.execute("SELECT COUNT(*) FROM Companies WHERE CompanyType = 'Buyer' AND MainEmail IS NULL")
    no_email = cursor.fetchone()[0]
    print(f"\n{no_email} buyer companies without email addresses")
    
    print("\n[SUCCESS] Import verification complete!")
    
    conn.close()
    
except Exception as e:
    print(f"[ERROR] {e}")
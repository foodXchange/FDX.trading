import json
import pyodbc
from datetime import datetime

def get_connection():
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

def clean_value(value, max_len=None):
    """Clean and truncate value for database"""
    if value is None:
        return ''
    str_val = str(value).strip()
    if str_val.lower() == 'nan' or str_val.lower() == 'none':
        return ''
    if max_len:
        return str_val[:max_len]
    return str_val

print("=" * 60)
print("IMPORTING EXHIBITORS DATA")
print("=" * 60)

# Load exhibitors JSON
file_path = r"G:\My Drive\Business intelligence\accessible_exhibitions_exhibitors.json"
print(f"\nLoading {file_path}...")

with open(file_path, 'r', encoding='utf-8') as f:
    exhibitors_data = json.load(f)

print(f"Found {len(exhibitors_data)} exhibitor records")

# Analyze structure
if exhibitors_data and len(exhibitors_data) > 0:
    print("\nSample exhibitor structure:")
    first = exhibitors_data[0]
    for key, value in first.items():
        print(f"  {key}: {str(value)[:50] if value else 'None'}")

conn = get_connection()
cursor = conn.cursor()

# Clear existing exhibitors
cursor.execute("DELETE FROM Exhibitors")
cursor.execute("DELETE FROM Exhibitions")
conn.commit()
print("\nCleared existing exhibitors and exhibitions")

exhibitions = {}
imported_exhibitors = 0
errors = 0

print("\nImporting exhibitors...")
for i, exhibitor in enumerate(exhibitors_data):
    try:
        # Get exhibition name (handle different possible field names)
        exhibition_name = clean_value(
            exhibitor.get('Exhibition') or 
            exhibitor.get('exhibition') or 
            exhibitor.get('Event') or 
            'Unknown Exhibition', 
            200
        )
        
        # Get or create exhibition
        if exhibition_name and exhibition_name not in exhibitions:
            cursor.execute("SELECT Id FROM Exhibitions WHERE Name = ?", (exhibition_name,))
            result = cursor.fetchone()
            
            if result:
                exhibitions[exhibition_name] = result[0]
            else:
                # Create new exhibition
                cursor.execute("""
                    INSERT INTO Exhibitions (Name, SourceUrl, CreatedAt) 
                    OUTPUT INSERTED.Id
                    VALUES (?, ?, ?)
                """, (
                    exhibition_name,
                    clean_value(exhibitor.get('source_url') or exhibitor.get('Source'), 500),
                    datetime.now()
                ))
                exhibitions[exhibition_name] = cursor.fetchone()[0]
                print(f"  Created exhibition: {exhibition_name}")
        
        # Get company name (handle different possible field names)
        company_name = clean_value(
            exhibitor.get('Company Name') or 
            exhibitor.get('company_name') or 
            exhibitor.get('Company') or 
            exhibitor.get('Name'), 
            200
        )
        
        if not company_name:
            continue
        
        # Get other fields
        country = clean_value(
            exhibitor.get('Country') or 
            exhibitor.get('country') or 
            exhibitor.get('Location'),
            100
        )
        
        products = clean_value(
            exhibitor.get('Products') or 
            exhibitor.get('products') or 
            exhibitor.get('Product Categories'),
            4000
        )
        
        contact = clean_value(
            exhibitor.get('Contact') or 
            exhibitor.get('contact') or 
            exhibitor.get('Contact Person'),
            500
        )
        
        profile_url = clean_value(
            exhibitor.get('Profile URL') or 
            exhibitor.get('profile_url') or 
            exhibitor.get('Website'),
            500
        )
        
        # Insert exhibitor
        cursor.execute("""
            INSERT INTO Exhibitors (
                CompanyName, ProfileUrl, Country, Products, 
                Contact, ExhibitionId, CreatedAt
            ) VALUES (?, ?, ?, ?, ?, ?, ?)
        """, (
            company_name,
            profile_url,
            country,
            products,
            contact,
            exhibitions.get(exhibition_name) if exhibition_name else None,
            datetime.now()
        ))
        imported_exhibitors += 1
        
        if imported_exhibitors % 10 == 0:
            print(f"  Imported {imported_exhibitors} exhibitors...")
            
    except Exception as e:
        errors += 1
        if errors <= 5:
            print(f"  Error on exhibitor {i}: {str(e)[:100]}")

conn.commit()

print(f"\n{'='*60}")
print(f"EXHIBITOR IMPORT COMPLETE")
print(f"{'='*60}")
print(f"Successfully imported: {imported_exhibitors} exhibitors")
print(f"Created exhibitions: {len(exhibitions)}")
print(f"Errors: {errors}")

# Verify
cursor.execute("SELECT COUNT(*) FROM Exhibitors")
exhibitor_count = cursor.fetchone()[0]
cursor.execute("SELECT COUNT(*) FROM Exhibitions")
exhibition_count = cursor.fetchone()[0]

print(f"\nDatabase now contains:")
print(f"  Exhibitors: {exhibitor_count}")
print(f"  Exhibitions: {exhibition_count}")

# Show samples
print("\nSample exhibitors:")
cursor.execute("""
    SELECT TOP 5 
        e.CompanyName, 
        e.Country, 
        ex.Name as Exhibition
    FROM Exhibitors e
    LEFT JOIN Exhibitions ex ON e.ExhibitionId = ex.Id
""")

for row in cursor.fetchall():
    print(f"  - {row[0][:30]:30} | {(row[1] or 'N/A')[:20]:20} | {(row[2] or 'N/A')[:30]}")

cursor.close()
conn.close()

print("\n[SUCCESS] Exhibitor import completed!")
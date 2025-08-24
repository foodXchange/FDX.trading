import json
import pyodbc
from datetime import datetime
import math

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
    if value is None or (isinstance(value, float) and math.isnan(value)):
        return ''
    str_val = str(value)
    if max_len:
        return str_val[:max_len]
    return str_val

print("Importing buyers with correct field mapping...")
file_path = r"G:\My Drive\Business intelligence\food_buyers_database.json"

with open(file_path, 'r', encoding='utf-8') as f:
    data = json.load(f)

conn = get_connection()
cursor = conn.cursor()

# Clear existing buyers
cursor.execute("DELETE FROM FoodXBuyers")
print("Cleared existing buyers")

buyers = data.get('all_buyers', [])
print(f"Found {len(buyers)} buyers in JSON file")

imported = 0
errors = 0

for i, buyer in enumerate(buyers):
    try:
        # Map JSON fields to database columns (note lowercase keys)
        company = clean_value(buyer.get('company'), 200)
        if not company:
            continue
            
        cursor.execute("""
            INSERT INTO FoodXBuyers (
                Company, Type, Website, Categories, Size, 
                Stores, Region, Markets, Domain,
                ProcurementEmail, ProcurementPhone, ProcurementManager,
                PaymentTerms, GeneralEmail, GeneralPhone, 
                CreatedAt
            ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        """, (
            company,
            clean_value(buyer.get('type'), 100),
            clean_value(buyer.get('website'), 500),
            clean_value(buyer.get('categories'), 200),
            clean_value(buyer.get('size'), 50),
            clean_value(buyer.get('stores'), 100),
            clean_value(buyer.get('region'), 100),
            clean_value(buyer.get('markets'), 200),
            clean_value(buyer.get('domain'), 200),
            clean_value(buyer.get('procurement_email'), 200),
            clean_value(buyer.get('procurement_phone'), 50),
            clean_value(buyer.get('procurement_manager'), 200),
            clean_value(buyer.get('payment_terms'), 200),
            clean_value(buyer.get('general_emails'), 200),
            clean_value(buyer.get('general_phones'), 50),
            datetime.now()
        ))
        imported += 1
        
        if imported % 50 == 0:
            print(f"Imported {imported} buyers...")
            conn.commit()
            
    except Exception as e:
        errors += 1
        if errors <= 5:
            print(f"Error on buyer {company}: {str(e)[:100]}")

conn.commit()
print(f"\n[SUCCESS] Imported {imported} buyers ({errors} errors)")

# Import exhibitors
print("\n" + "="*50)
print("Importing exhibitors...")
file_path = r"G:\My Drive\Business intelligence\accessible_exhibitions_exhibitors.json"

with open(file_path, 'r', encoding='utf-8') as f:
    exhibitors = json.load(f)

print(f"Found {len(exhibitors)} exhibitors")

exhibitions = {}
imported_ex = 0
errors_ex = 0

for exhibitor in exhibitors:
    try:
        exhibition_name = clean_value(exhibitor.get('Exhibition'), 200)
        
        # Get or create exhibition
        if exhibition_name and exhibition_name not in exhibitions:
            cursor.execute("SELECT Id FROM Exhibitions WHERE Name = ?", (exhibition_name,))
            result = cursor.fetchone()
            
            if result:
                exhibitions[exhibition_name] = result[0]
            else:
                cursor.execute("""
                    INSERT INTO Exhibitions (Name, SourceUrl, CreatedAt) 
                    VALUES (?, ?, ?)
                """, (
                    exhibition_name,
                    clean_value(exhibitor.get('source_url'), 500),
                    datetime.now()
                ))
                cursor.execute("SELECT @@IDENTITY")
                exhibitions[exhibition_name] = cursor.fetchone()[0]
        
        # Insert exhibitor
        company_name = clean_value(exhibitor.get('Company Name'), 200)
        if not company_name:
            continue
            
        cursor.execute("""
            INSERT INTO Exhibitors (
                CompanyName, ProfileUrl, Country, Products, 
                Contact, ExhibitionId, CreatedAt
            ) VALUES (?, ?, ?, ?, ?, ?, ?)
        """, (
            company_name,
            clean_value(exhibitor.get('Profile URL'), 500),
            clean_value(exhibitor.get('Country'), 100),
            clean_value(exhibitor.get('Products'), 4000),
            clean_value(exhibitor.get('Contact'), 500),
            exhibitions.get(exhibition_name) if exhibition_name else None,
            datetime.now()
        ))
        imported_ex += 1
        
    except Exception as e:
        errors_ex += 1
        if errors_ex <= 5:
            print(f"Error on exhibitor: {str(e)[:100]}")

conn.commit()
print(f"[SUCCESS] Imported {imported_ex} exhibitors ({errors_ex} errors)")
print(f"Created/found {len(exhibitions)} exhibitions")

# Final verification
print("\n" + "="*50)
print("FINAL VERIFICATION:")
cursor.execute("SELECT COUNT(*) FROM FoodXBuyers")
buyer_count = cursor.fetchone()[0]
cursor.execute("SELECT COUNT(*) FROM FoodXSuppliers")
supplier_count = cursor.fetchone()[0]
cursor.execute("SELECT COUNT(*) FROM Exhibitors")
exhibitor_count = cursor.fetchone()[0]
cursor.execute("SELECT COUNT(*) FROM Exhibitions")
exhibition_count = cursor.fetchone()[0]

print(f"  FoodXBuyers: {buyer_count:,} records")
print(f"  FoodXSuppliers: {supplier_count:,} records")
print(f"  Exhibitors: {exhibitor_count:,} records")
print(f"  Exhibitions: {exhibition_count:,} records")

cursor.close()
conn.close()
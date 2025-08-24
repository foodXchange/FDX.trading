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
    str_val = str(value).strip()
    if max_len:
        return str_val[:max_len]
    return str_val

print("="*60)
print("IMPORTING ALL SUPPLIERS")
print("="*60)

file_path = r"G:\My Drive\Business intelligence\food_suppliers_database.json"

print(f"\nLoading {file_path}...")
with open(file_path, 'r', encoding='utf-8') as f:
    data = json.load(f)

suppliers = data.get('suppliers', [])
print(f"Found {len(suppliers):,} suppliers in JSON file")

conn = get_connection()
cursor = conn.cursor()

# Clear existing suppliers
print("\nClearing existing suppliers...")
cursor.execute("DELETE FROM FoodXSuppliers")
conn.commit()
print("Cleared existing suppliers")

imported = 0
errors = 0
batch_size = 100

print("\nStarting import...")
for i, supplier in enumerate(suppliers):
    try:
        # Get supplier name from multiple possible fields
        supplier_name = (clean_value(supplier.get('Supplier Name'), 200) or 
                        clean_value(supplier.get('Company Name'), 200) or 
                        clean_value(supplier.get('CompanyName'), 200))
        
        if not supplier_name:
            continue
            
        cursor.execute("""
            INSERT INTO FoodXSuppliers (
                SupplierName, CompanyWebsite, Description,
                ProductCategory, Address, CompanyEmail,
                Phone, Products, Country, PaymentTerms,
                Incoterms, ContactPerson, ContactEmail, ContactPhone,
                CreatedAt
            ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        """, (
            supplier_name,
            clean_value(supplier.get('Company website') or supplier.get('Open Website'), 500),
            clean_value(supplier.get("Supplier's Description & Products") or supplier.get('Description'), 4000),
            clean_value(supplier.get('Product Category & family (Txt)') or supplier.get('Product Category'), 200),
            clean_value(supplier.get('Address') or supplier.get('Company Address'), 500),
            clean_value(supplier.get('Company Email') or supplier.get('Email'), 200),
            clean_value(supplier.get('Phone') or supplier.get('Tel'), 50),
            clean_value(supplier.get('Products'), 4000),
            clean_value(supplier.get('Country'), 100),
            clean_value(supplier.get('Terms of Payment') or supplier.get('Payment Terms'), 200),
            clean_value(supplier.get('Incoterms'), 100),
            clean_value(supplier.get('Contact Person') or supplier.get('Contact'), 200),
            clean_value(supplier.get('Contact Email'), 200),
            clean_value(supplier.get('Contact Phone'), 50),
            datetime.now()
        ))
        imported += 1
        
        # Commit in batches
        if imported % batch_size == 0:
            conn.commit()
            if imported % 1000 == 0:
                print(f"  Imported {imported:,} suppliers...")
            
    except Exception as e:
        errors += 1
        if errors <= 10:  # Show first 10 errors
            print(f"  Error on supplier {i}: {str(e)[:100]}")
        elif errors == 11:
            print(f"  (Suppressing further error messages...)")

# Final commit
conn.commit()

print(f"\n{'='*60}")
print(f"IMPORT COMPLETE")
print(f"{'='*60}")
print(f"Successfully imported: {imported:,} suppliers")
print(f"Errors encountered: {errors:,}")

# Verify final count
cursor.execute("SELECT COUNT(*) FROM FoodXSuppliers")
db_count = cursor.fetchone()[0]
print(f"Total suppliers in database: {db_count:,}")

# Show sample
print("\nSample imported suppliers:")
cursor.execute("""
    SELECT TOP 5 SupplierName, Country, ProductCategory 
    FROM FoodXSuppliers 
    ORDER BY Id DESC
""")
for row in cursor.fetchall():
    print(f"  - {row[0][:40]:40} | {(row[1] or 'N/A')[:20]:20} | {(row[2] or 'N/A')[:30]}")

cursor.close()
conn.close()

print("\n[SUCCESS] Import completed!")
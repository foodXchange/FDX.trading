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
    if value is None or (isinstance(value, float) and math.isnan(value)):
        return ''
    str_val = str(value).strip()
    if max_len:
        return str_val[:max_len]
    return str_val

print("Continuing supplier import...")

# Get existing supplier names to avoid duplicates
conn = get_connection()
cursor = conn.cursor()

cursor.execute("SELECT COUNT(*) FROM FoodXSuppliers")
existing_count = cursor.fetchone()[0]
print(f"Currently have {existing_count:,} suppliers in database")

# Get existing supplier names
print("Loading existing supplier names...")
cursor.execute("SELECT SupplierName FROM FoodXSuppliers")
existing_names = set(row[0] for row in cursor.fetchall())
print(f"Found {len(existing_names):,} unique supplier names")

# Load JSON
file_path = r"G:\My Drive\Business intelligence\food_suppliers_database.json"
print(f"\nLoading JSON file...")
with open(file_path, 'r', encoding='utf-8') as f:
    data = json.load(f)

suppliers = data.get('suppliers', [])
print(f"Total suppliers in JSON: {len(suppliers):,}")

imported = 0
skipped = 0
errors = 0
batch_size = 50

print("\nImporting remaining suppliers...")
for i, supplier in enumerate(suppliers):
    try:
        supplier_name = (clean_value(supplier.get('Supplier Name'), 200) or 
                        clean_value(supplier.get('Company Name'), 200) or 
                        clean_value(supplier.get('CompanyName'), 200))
        
        if not supplier_name:
            continue
            
        # Skip if already exists
        if supplier_name in existing_names:
            skipped += 1
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
        existing_names.add(supplier_name)
        
        if imported % batch_size == 0:
            conn.commit()
            if imported % 500 == 0:
                print(f"  Newly imported: {imported:,} (skipped: {skipped:,})")
            
    except Exception as e:
        errors += 1
        if errors <= 5:
            print(f"  Error: {str(e)[:100]}")

conn.commit()

# Final count
cursor.execute("SELECT COUNT(*) FROM FoodXSuppliers")
final_count = cursor.fetchone()[0]

print(f"\n{'='*60}")
print(f"IMPORT COMPLETE")
print(f"{'='*60}")
print(f"Newly imported: {imported:,}")
print(f"Skipped (duplicates): {skipped:,}")
print(f"Errors: {errors:,}")
print(f"Total suppliers in database: {final_count:,}")

cursor.close()
conn.close()
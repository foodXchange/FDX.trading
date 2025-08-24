import json
import pyodbc
import os
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

print("=" * 80)
print("CHECKING ENHANCED SUPPLIERS FILE")
print("=" * 80)

# Check if enhanced file exists
enhanced_file = r"G:\My Drive\Business intelligence\suppliers_enhanced_complete.json"

if not os.path.exists(enhanced_file):
    print(f"File not found: {enhanced_file}")
    exit()

file_size = os.path.getsize(enhanced_file) / 1024 / 1024
print(f"\nFile found: {enhanced_file}")
print(f"Size: {file_size:.2f} MB")

# Load the enhanced data
print("\nLoading enhanced suppliers data...")
with open(enhanced_file, 'r', encoding='utf-8') as f:
    enhanced_data = json.load(f)

# Analyze structure
print("\nFile structure:")
if isinstance(enhanced_data, dict):
    for key in enhanced_data.keys():
        if isinstance(enhanced_data[key], list):
            print(f"  '{key}': {len(enhanced_data[key])} items")
        else:
            print(f"  '{key}': {type(enhanced_data[key]).__name__}")
    
    # Get suppliers list
    suppliers = enhanced_data.get('suppliers', enhanced_data.get('data', []))
elif isinstance(enhanced_data, list):
    suppliers = enhanced_data
    print(f"  Direct list of {len(suppliers)} suppliers")
else:
    print(f"  Unknown structure: {type(enhanced_data).__name__}")
    suppliers = []

if suppliers and len(suppliers) > 0:
    print(f"\nTotal enhanced suppliers: {len(suppliers):,}")
    
    # Analyze first supplier to see enhanced fields
    print("\nEnhanced data fields (from first supplier):")
    first = suppliers[0]
    for key, value in first.items():
        value_type = type(value).__name__
        if value is None:
            print(f"  {key}: None")
        elif isinstance(value, str):
            print(f"  {key}: string ({len(value)} chars)")
        elif isinstance(value, list):
            print(f"  {key}: list ({len(value)} items)")
        elif isinstance(value, dict):
            print(f"  {key}: dict ({len(value)} keys)")
        else:
            print(f"  {key}: {value_type}")
    
    # Check what's new/different
    print("\nComparing with current database...")
    
    conn = get_connection()
    cursor = conn.cursor()
    
    # Get current supplier count
    cursor.execute("SELECT COUNT(*) FROM FoodXSuppliers")
    db_count = cursor.fetchone()[0]
    print(f"  Current suppliers in database: {db_count:,}")
    
    # Get current supplier names for comparison
    cursor.execute("SELECT SupplierName FROM FoodXSuppliers")
    db_suppliers = set(row[0] for row in cursor.fetchall())
    
    # Check for new suppliers in enhanced file
    new_suppliers = []
    updated_fields = set()
    
    for supplier in suppliers[:100]:  # Check first 100 for analysis
        name = (supplier.get('SupplierName') or 
                supplier.get('Supplier Name') or 
                supplier.get('CompanyName') or 
                supplier.get('Company Name') or 
                supplier.get('name', ''))
        
        if name and name not in db_suppliers:
            new_suppliers.append(name)
        
        # Collect all field names
        updated_fields.update(supplier.keys())
    
    print(f"\n  New suppliers found (first 100): {len(new_suppliers)}")
    print(f"  Total unique fields in enhanced data: {len(updated_fields)}")
    
    # Show enhanced fields not in basic data
    enhanced_only_fields = [f for f in updated_fields if any(keyword in f.lower() for keyword in 
                            ['enhanced', 'verified', 'rating', 'certification', 'quality', 
                             'export', 'capacity', 'lead', 'minimum', 'contact'])]
    
    if enhanced_only_fields:
        print("\n  Potentially enhanced fields:")
        for field in sorted(enhanced_only_fields)[:15]:
            print(f"    - {field}")
    
    # Check data quality improvements
    print("\nData quality analysis (first 100 suppliers):")
    
    with_email = 0
    with_phone = 0
    with_website = 0
    with_detailed_products = 0
    with_certifications = 0
    
    for supplier in suppliers[:100]:
        if supplier.get('CompanyEmail') or supplier.get('Email'):
            with_email += 1
        if supplier.get('Phone') or supplier.get('ContactPhone'):
            with_phone += 1
        if supplier.get('Website') or supplier.get('CompanyWebsite'):
            with_website += 1
        if supplier.get('Products') and len(str(supplier.get('Products', ''))) > 100:
            with_detailed_products += 1
        if supplier.get('Certifications') or supplier.get('QualityCertifications'):
            with_certifications += 1
    
    print(f"  With email: {with_email}%")
    print(f"  With phone: {with_phone}%")
    print(f"  With website: {with_website}%")
    print(f"  With detailed products: {with_detailed_products}%")
    print(f"  With certifications: {with_certifications}%")
    
    cursor.close()
    conn.close()
    
    print("\n" + "=" * 80)
    print("RECOMMENDATIONS:")
    print("-" * 40)
    print("1. Enhanced file contains additional/improved supplier data")
    print("2. Can update existing suppliers with enhanced fields")
    print("3. Can add new suppliers not in current database")
    print("4. Enhanced data includes quality/certification info")
    print("\nWould you like to:")
    print("  - Update existing suppliers with enhanced data?")
    print("  - Import new suppliers from enhanced file?")
    print("  - Add enhanced fields to database schema?")

else:
    print("\nNo supplier data found in enhanced file")
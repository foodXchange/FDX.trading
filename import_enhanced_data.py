import json
import pyodbc
from datetime import datetime
import math
from db_connection import get_connection

def clean_value(value, max_len=None):
    """Clean and prepare value for database"""
    if value is None or (isinstance(value, float) and math.isnan(value)):
        return None
    
    str_val = str(value).strip()
    if str_val.lower() in ['nan', 'none', '']:
        return None
    
    if max_len and len(str_val) > max_len:
        return str_val[:max_len]
    return str_val

def parse_datetime(date_str):
    """Parse datetime from various formats"""
    if not date_str:
        return None
    try:
        # Try different date formats
        for fmt in ['%Y-%m-%d %H:%M:%S', '%Y-%m-%d', '%m/%d/%Y', '%d/%m/%Y']:
            try:
                return datetime.strptime(str(date_str), fmt)
            except:
                continue
    except:
        pass
    return None

print("=" * 80)
print("IMPORTING ENHANCED SUPPLIER DATA")
print("=" * 80)

# Load enhanced data
enhanced_file = r"G:\My Drive\Business intelligence\suppliers_enhanced_complete.json"
print(f"\nLoading {enhanced_file}...")

with open(enhanced_file, 'r', encoding='utf-8') as f:
    data = json.load(f)

suppliers = data.get('suppliers', [])
print(f"Found {len(suppliers):,} enhanced supplier records")

conn = get_connection()
cursor = conn.cursor()

# Get existing suppliers for matching
print("\nLoading existing suppliers for matching...")
cursor.execute("SELECT SupplierName, Id FROM FoodXSuppliers")
existing_suppliers = {row[0]: row[1] for row in cursor.fetchall()}
print(f"Found {len(existing_suppliers):,} existing suppliers")

updated = 0
new_added = 0
errors = 0
batch_size = 50

print("\nProcessing enhanced data...")
for i, supplier in enumerate(suppliers):
    try:
        # Get supplier name
        supplier_name = clean_value(
            supplier.get('Supplier Name') or 
            supplier.get('Company Name') or 
            supplier.get('CompanyName'), 200
        )
        
        if not supplier_name:
            continue
        
        # Check if supplier exists
        if supplier_name in existing_suppliers:
            # Update existing supplier with enhanced data
            supplier_id = existing_suppliers[supplier_name]
            
            cursor.execute("""
                UPDATE FoodXSuppliers SET
                    ProductsList = COALESCE(ProductsList, ?),
                    BrandsList = COALESCE(BrandsList, ?),
                    Categories = COALESCE(Categories, ?),
                    KosherCertification = COALESCE(KosherCertification, ?),
                    OtherCertifications = COALESCE(OtherCertifications, ?),
                    PrimaryEmail = COALESCE(PrimaryEmail, ?),
                    AllEmails = COALESCE(AllEmails, ?),
                    PrimaryPhone = COALESCE(PrimaryPhone, ?),
                    ClosestSeaPort = COALESCE(ClosestSeaPort, ?),
                    DistanceToSeaPort = COALESCE(DistanceToSeaPort, ?),
                    SupplierCode = COALESCE(SupplierCode, ?),
                    SupplierVATNumber = COALESCE(SupplierVATNumber, ?),
                    YearFounded = COALESCE(YearFounded, ?),
                    SupplierType = COALESCE(SupplierType, ?),
                    PickingAddress = COALESCE(PickingAddress, ?),
                    IncotermsPrice = COALESCE(IncotermsPrice, ?),
                    SourcingStages = COALESCE(SourcingStages, ?),
                    ExtractionStatus = COALESCE(ExtractionStatus, ?),
                    ProductsFound = COALESCE(ProductsFound, ?),
                    BrandsFound = COALESCE(BrandsFound, ?),
                    DataSource = COALESCE(DataSource, ?),
                    FirstCreated = COALESCE(FirstCreated, ?),
                    LastUpdated = COALESCE(LastUpdated, ?),
                    CompanyLogoURL = COALESCE(CompanyLogoURL, ?),
                    ProfileImages = COALESCE(ProfileImages, ?)
                WHERE Id = ?
            """, (
                clean_value(supplier.get('products_list')),
                clean_value(supplier.get('brands_list')),
                clean_value(supplier.get('categories')),
                1 if supplier.get('Kosher certification') == 'Yes' else 0 if supplier.get('Kosher certification') else None,
                clean_value(supplier.get('certifications')),
                clean_value(supplier.get('primary_email'), 200),
                clean_value(supplier.get('all_emails')),
                clean_value(supplier.get('primary_phone'), 50),
                clean_value(supplier.get('Closest/ Prefered SeaPort'), 200),
                int(supplier.get('Distance to Seaport (km)')) if supplier.get('Distance to Seaport (km)') else None,
                clean_value(supplier.get('Supplier Code'), 100),
                clean_value(supplier.get("Supplier's VAT number"), 100),
                int(supplier.get('year_founded')) if supplier.get('year_founded') else None,
                clean_value(supplier.get('Supplier Type'), 100),
                clean_value(supplier.get('If EXW  insert picking address including zip code'), 500),
                clean_value(supplier.get('Incoterms (Price Base)'), 100),
                clean_value(supplier.get('Sourcing stages'), 200),
                clean_value(supplier.get('extraction_status'), 50),
                int(supplier.get('products_found')) if supplier.get('products_found') else None,
                int(supplier.get('brands_found')) if supplier.get('brands_found') else None,
                clean_value(supplier.get('Source of data'), 200),
                parse_datetime(supplier.get('First Created')),
                parse_datetime(supplier.get('Last Updated')),
                clean_value(supplier.get('Company logo URL'), 500),
                clean_value(supplier.get('Profile images')),
                supplier_id
            ))
            updated += 1
            
        else:
            # Add as new supplier
            cursor.execute("""
                INSERT INTO FoodXSuppliers (
                    SupplierName, CompanyWebsite, Description, ProductCategory,
                    Address, CompanyEmail, Phone, Products, Country,
                    ProductsList, BrandsList, Categories, KosherCertification,
                    OtherCertifications, PrimaryEmail, AllEmails, PrimaryPhone,
                    ClosestSeaPort, DistanceToSeaPort, SupplierCode, SupplierVATNumber,
                    YearFounded, SupplierType, PickingAddress, IncotermsPrice,
                    SourcingStages, ExtractionStatus, ProductsFound, BrandsFound,
                    DataSource, FirstCreated, LastUpdated, CompanyLogoURL,
                    ProfileImages, CreatedAt
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?,
                         ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            """, (
                supplier_name,
                clean_value(supplier.get('Company website') or supplier.get('Open Website'), 500),
                clean_value(supplier.get("Supplier's Description & Products")),
                clean_value(supplier.get('Product Category & family (Txt)'), 200),
                clean_value(supplier.get('Address') or supplier.get('Company address'), 500),
                clean_value(supplier.get('Company Email'), 200),
                clean_value(supplier.get('Phone'), 50),
                clean_value(supplier.get('Products')),
                clean_value(supplier.get("Supplier's Country") or supplier.get('country'), 100),
                clean_value(supplier.get('products_list')),
                clean_value(supplier.get('brands_list')),
                clean_value(supplier.get('categories')),
                1 if supplier.get('Kosher certification') == 'Yes' else 0 if supplier.get('Kosher certification') else None,
                clean_value(supplier.get('certifications')),
                clean_value(supplier.get('primary_email'), 200),
                clean_value(supplier.get('all_emails')),
                clean_value(supplier.get('primary_phone'), 50),
                clean_value(supplier.get('Closest/ Prefered SeaPort'), 200),
                int(supplier.get('Distance to Seaport (km)')) if supplier.get('Distance to Seaport (km)') else None,
                clean_value(supplier.get('Supplier Code'), 100),
                clean_value(supplier.get("Supplier's VAT number"), 100),
                int(supplier.get('year_founded')) if supplier.get('year_founded') else None,
                clean_value(supplier.get('Supplier Type'), 100),
                clean_value(supplier.get('If EXW  insert picking address including zip code'), 500),
                clean_value(supplier.get('Incoterms (Price Base)'), 100),
                clean_value(supplier.get('Sourcing stages'), 200),
                clean_value(supplier.get('extraction_status'), 50),
                int(supplier.get('products_found')) if supplier.get('products_found') else None,
                int(supplier.get('brands_found')) if supplier.get('brands_found') else None,
                clean_value(supplier.get('Source of data'), 200),
                parse_datetime(supplier.get('First Created')),
                parse_datetime(supplier.get('Last Updated')),
                clean_value(supplier.get('Company logo URL'), 500),
                clean_value(supplier.get('Profile images')),
                datetime.now()
            ))
            new_added += 1
            existing_suppliers[supplier_name] = cursor.lastrowid
        
        # Commit in batches
        if (updated + new_added) % batch_size == 0:
            conn.commit()
            if (updated + new_added) % 500 == 0:
                print(f"  Processed {updated + new_added:,} records (Updated: {updated:,}, New: {new_added:,})")
                
    except Exception as e:
        errors += 1
        if errors <= 10:
            print(f"  Error on supplier {i}: {str(e)[:100]}")

# Final commit
conn.commit()

print(f"\n{'='*80}")
print("ENHANCED DATA IMPORT COMPLETE")
print(f"{'='*80}")
print(f"Updated existing suppliers: {updated:,}")
print(f"Added new suppliers: {new_added:,}")
print(f"Errors: {errors:,}")

# Verify results
cursor.execute("SELECT COUNT(*) FROM FoodXSuppliers")
total = cursor.fetchone()[0]

cursor.execute("SELECT COUNT(*) FROM FoodXSuppliers WHERE ProductsList IS NOT NULL")
with_products_list = cursor.fetchone()[0]

cursor.execute("SELECT COUNT(*) FROM FoodXSuppliers WHERE KosherCertification = 1")
kosher = cursor.fetchone()[0]

cursor.execute("SELECT COUNT(*) FROM FoodXSuppliers WHERE OtherCertifications IS NOT NULL")
certified = cursor.fetchone()[0]

print(f"\nDatabase now contains:")
print(f"  Total suppliers: {total:,}")
print(f"  With enhanced product lists: {with_products_list:,}")
print(f"  Kosher certified: {kosher:,}")
print(f"  With certifications: {certified:,}")

cursor.close()
conn.close()

print("\n[SUCCESS] Enhanced data import completed!")
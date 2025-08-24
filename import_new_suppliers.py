#!/usr/bin/env python
"""
Import new suppliers from master_suppliers_database.json to FoodX database
Compares existing suppliers and imports only new records
"""
import json
import sys
from db_connection import get_connection

def load_json_suppliers(file_path):
    """Load suppliers from JSON file"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        suppliers = data.get('suppliers', [])
        print(f"Loaded {len(suppliers)} suppliers from JSON file")
        return suppliers
    except Exception as e:
        print(f"Error loading JSON file: {e}")
        return []

def get_existing_suppliers():
    """Get existing suppliers from database"""
    try:
        conn = get_connection()
        cursor = conn.cursor()
        
        # Get existing supplier names and websites for comparison
        cursor.execute("""
            SELECT SupplierName, CompanyWebsite 
            FROM FoodXSuppliers 
            WHERE SupplierName IS NOT NULL
        """)
        
        existing = set()
        rows = cursor.fetchall()
        for row in rows:
            supplier_name = row[0].strip().lower() if row[0] else ""
            website = row[1].strip().lower() if row[1] else ""
            if supplier_name:
                existing.add(supplier_name)
            if website:
                existing.add(website)
        
        print(f"Found {len(existing)} existing supplier identifiers")
        cursor.close()
        conn.close()
        return existing
        
    except Exception as e:
        print(f"Error getting existing suppliers: {e}")
        return set()

def identify_new_suppliers(json_suppliers, existing_suppliers):
    """Compare and identify new suppliers"""
    new_suppliers = []
    
    for supplier in json_suppliers:
        company_name = supplier.get('company', '').strip().lower()
        website = supplier.get('contact', {}).get('website', '').strip().lower()
        
        # Check if supplier already exists by name or website
        is_existing = False
        if company_name and company_name in existing_suppliers:
            is_existing = True
        if website and website in existing_suppliers:
            is_existing = True
            
        if not is_existing:
            new_suppliers.append(supplier)
    
    print(f"Identified {len(new_suppliers)} new suppliers to import")
    return new_suppliers

def prepare_supplier_for_import(supplier):
    """Transform JSON supplier to database format"""
    contact = supplier.get('contact', {})
    location = supplier.get('location', {})
    products = supplier.get('products', {})
    certifications = supplier.get('certifications', {})
    
    # Create address from location data
    address_parts = []
    if location.get('address'):
        address_parts.append(location.get('address'))
    if location.get('plant_locations'):
        address_parts.append(f"Plants: {location.get('plant_locations')}")
    address = "; ".join(address_parts) if address_parts else ""
    
    # Create comprehensive description
    desc_parts = []
    desc_parts.append(f"Type: {supplier.get('entity_type', 'Unknown')}")
    if products.get('product_types'):
        desc_parts.append(f"Products: {products.get('product_types')}")
    if products.get('brands'):
        desc_parts.append(f"Brands: {products.get('brands')}")
    if products.get('export_regions'):
        desc_parts.append(f"Export Regions: {products.get('export_regions')}")
    
    # Add certifications to description
    cert_list = []
    if certifications.get('iso_22000'):
        cert_list.append("ISO 22000")
    if certifications.get('haccp'):
        cert_list.append("HACCP")
    if certifications.get('ifs'):
        cert_list.append("IFS")
    if certifications.get('brc'):
        cert_list.append("BRC")
    if certifications.get('fssc_22000'):
        cert_list.append("FSSC 22000")
    if certifications.get('organic'):
        cert_list.append("Organic")
    if certifications.get('kosher'):
        cert_list.append("Kosher")
    if certifications.get('halal'):
        cert_list.append("Halal")
    
    if cert_list:
        desc_parts.append(f"Certifications: {', '.join(cert_list)}")
    
    description = " | ".join(desc_parts)
    
    return {
        'SupplierName': supplier.get('company', '')[:255] if supplier.get('company') else '',
        'CompanyWebsite': contact.get('website', '')[:500] if contact.get('website') else '',
        'Description': description[:2000] if description else '',
        'ProductCategory': supplier.get('category', '')[:200] if supplier.get('category') else '',
        'Address': address[:500] if address else '',
        'CompanyEmail': contact.get('email', '')[:200] if contact.get('email') else '',
        'Phone': contact.get('phone', '')[:50] if contact.get('phone') else '',
        'Products': products.get('product_types', '')[:1000] if products.get('product_types') else '',
        'Country': location.get('country', '')[:100] if location.get('country') else '',
    }

def import_suppliers_batch(suppliers_data, batch_size=50):
    """Import suppliers in batches"""
    try:
        conn = get_connection()
        cursor = conn.cursor()
        
        # FoodXSuppliers table uses auto-increment identity column
        
        total_imported = 0
        
        for i in range(0, len(suppliers_data), batch_size):
            batch = suppliers_data[i:i + batch_size]
            
            # Insert one by one for simplicity and better error handling
            for supplier in batch:
                try:
                    # Let the database auto-generate the ID
                    insert_sql = """
                        INSERT INTO FoodXSuppliers (
                            SupplierName, CompanyWebsite, Description, ProductCategory, 
                            Address, CompanyEmail, Phone, Products, Country
                        ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
                    """
                    
                    # Clean string values to avoid encoding issues
                    def clean_string(s):
                        if not s:
                            return s
                        # Replace problematic characters
                        return s.encode('ascii', 'ignore').decode('ascii').strip()
                    
                    params = [
                        clean_string(supplier['SupplierName']),
                        clean_string(supplier['CompanyWebsite']),
                        clean_string(supplier['Description']),
                        clean_string(supplier['ProductCategory']),
                        clean_string(supplier['Address']),
                        clean_string(supplier['CompanyEmail']),
                        clean_string(supplier['Phone']),
                        clean_string(supplier['Products']),
                        clean_string(supplier['Country'])
                    ]
                    
                    cursor.execute(insert_sql, params)
                    total_imported += 1
                    
                except Exception as e:
                    print(f"Error inserting supplier {supplier.get('SupplierName', 'Unknown')}: {e}")
                    continue
            
            print(f"Processed batch {i//batch_size + 1}: {len(batch)} suppliers (Total imported: {total_imported})")
        
        conn.commit()
        cursor.close()
        conn.close()
        
        print(f"Successfully imported {total_imported} new suppliers")
        return total_imported
        
    except Exception as e:
        print(f"Error importing suppliers: {e}")
        return 0

def main():
    """Main import process"""
    json_file = r"G:\My Drive\Business intelligence\master_suppliers_database.json"
    
    print("Starting supplier import process...")
    print("=" * 50)
    
    # Step 1: Load JSON data
    print("1. Loading JSON suppliers...")
    json_suppliers = load_json_suppliers(json_file)
    if not json_suppliers:
        print("No suppliers found in JSON file")
        return False
    
    # Step 2: Get existing suppliers
    print("2. Getting existing suppliers from database...")
    existing_suppliers = get_existing_suppliers()
    
    # Step 3: Identify new suppliers
    print("3. Comparing and identifying new suppliers...")
    new_suppliers = identify_new_suppliers(json_suppliers, existing_suppliers)
    
    if not new_suppliers:
        print("No new suppliers to import!")
        return True
    
    # Step 4: Prepare data for import
    print("4. Preparing supplier data for import...")
    prepared_suppliers = []
    for supplier in new_suppliers:
        try:
            prepared = prepare_supplier_for_import(supplier)
            prepared_suppliers.append(prepared)
        except Exception as e:
            print(f"Error preparing supplier {supplier.get('company', 'Unknown')}: {e}")
    
    print(f"Prepared {len(prepared_suppliers)} suppliers for import")
    
    # Step 5: Import suppliers
    print("5. Importing new suppliers to database...")
    imported_count = import_suppliers_batch(prepared_suppliers)
    
    print("=" * 50)
    print(f"Import completed!")
    print(f"Total suppliers in JSON: {len(json_suppliers)}")
    print(f"New suppliers identified: {len(new_suppliers)}")
    print(f"Successfully imported: {imported_count}")
    
    return imported_count > 0

if __name__ == "__main__":
    success = main()
    sys.exit(0 if success else 1)
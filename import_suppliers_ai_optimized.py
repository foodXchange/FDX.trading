#!/usr/bin/env python
"""
Import suppliers from suppliers_ai_optimized.json to FoodX database
This file contains 14,626 suppliers with 12,785 having product data
"""
import json
import sys
from db_connection import get_connection

def load_ai_optimized_suppliers(file_path):
    """Load suppliers from AI-optimized JSON file"""
    try:
        print(f"Loading {file_path}...")
        with open(file_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        metadata = data.get('metadata', {})
        suppliers = data.get('suppliers', [])
        
        print(f"File metadata:")
        print(f"  Total suppliers: {metadata.get('total_suppliers', 0):,}")
        print(f"  Suppliers with products: {metadata.get('suppliers_with_products', 0):,}")
        print(f"  Loaded {len(suppliers):,} suppliers from file")
        
        return suppliers
    except Exception as e:
        print(f"Error loading JSON file: {e}")
        return []

def get_existing_suppliers():
    """Get existing suppliers from database for deduplication"""
    try:
        conn = get_connection()
        cursor = conn.cursor()
        
        # Get count
        cursor.execute("SELECT COUNT(*) FROM FoodXSuppliers")
        count = cursor.fetchone()[0]
        print(f"\nCurrent suppliers in database: {count:,}")
        
        # Get existing supplier names and websites
        cursor.execute("""
            SELECT SupplierName, CompanyWebsite 
            FROM FoodXSuppliers 
            WHERE SupplierName IS NOT NULL
        """)
        
        existing = {}
        for row in cursor.fetchall():
            name = row[0].strip().lower() if row[0] else ""
            website = row[1].strip().lower() if row[1] else ""
            if name:
                existing[name] = True
            if website:
                existing[website] = True
        
        print(f"Found {len(existing):,} unique identifiers")
        cursor.close()
        conn.close()
        return existing
        
    except Exception as e:
        print(f"Error getting existing suppliers: {e}")
        return {}

def prepare_supplier_for_db(supplier):
    """Transform AI-optimized supplier to database format"""
    company = supplier.get('company', {})
    products_list = supplier.get('products', [])
    brands_list = supplier.get('brands', [])
    categories_list = supplier.get('category', [])
    
    # Join products with semicolon for storage
    products_str = "; ".join(products_list[:50])  # Limit to first 50 products
    
    # Join categories
    category_str = " - ".join(categories_list) if categories_list else ""
    
    # Create description with brands
    description_parts = []
    if brands_list:
        description_parts.append(f"Brands: {', '.join(brands_list)}")
    if supplier.get('has_products'):
        description_parts.append(f"Products available: {len(products_list)}")
    
    description = " | ".join(description_parts)
    
    return {
        'supplier_id': supplier.get('supplier_id', ''),
        'name': company.get('name', ''),
        'website': company.get('website', ''),
        'email': company.get('email', ''),
        'country': company.get('country', ''),
        'products': products_str,
        'category': category_str,
        'description': description,
        'search_text': supplier.get('search_text', '')[:2000],  # Limit search text
        'has_products': supplier.get('has_products', False)
    }

def clean_existing_data(conn):
    """Clean demo/test data from FoodXSuppliers"""
    try:
        cursor = conn.cursor()
        
        # Count demo data
        cursor.execute("""
            SELECT COUNT(*) FROM FoodXSuppliers 
            WHERE SupplierName LIKE '%test%' 
            OR SupplierName LIKE '%demo%'
            OR SupplierName LIKE '%sample%'
        """)
        demo_count = cursor.fetchone()[0]
        
        if demo_count > 0:
            print(f"\nRemoving {demo_count} demo/test suppliers...")
            cursor.execute("""
                DELETE FROM FoodXSuppliers 
                WHERE SupplierName LIKE '%test%' 
                OR SupplierName LIKE '%demo%'
                OR SupplierName LIKE '%sample%'
            """)
            conn.commit()
            print(f"Removed {demo_count} demo records")
        else:
            print("\nNo demo/test data found")
        
        cursor.close()
        return True
    except Exception as e:
        print(f"Error cleaning data: {e}")
        return False

def import_suppliers_batch(suppliers_data, existing, batch_size=100):
    """Import suppliers in batches"""
    try:
        conn = get_connection()
        
        # Clean demo data first
        clean_existing_data(conn)
        
        cursor = conn.cursor()
        
        total_imported = 0
        total_updated = 0
        total_skipped = 0
        
        for i in range(0, len(suppliers_data), batch_size):
            batch = suppliers_data[i:i + batch_size]
            
            for supplier in batch:
                try:
                    prepared = prepare_supplier_for_db(supplier)
                    
                    # Check if exists
                    name_lower = prepared['name'].lower()
                    website_lower = prepared['website'].lower()
                    
                    if name_lower in existing or website_lower in existing:
                        # Update existing
                        if prepared['has_products'] and prepared['products']:
                            cursor.execute("""
                                UPDATE FoodXSuppliers 
                                SET Products = ?, 
                                    ProductCategory = ?,
                                    Description = ?,
                                    CompanyEmail = CASE WHEN CompanyEmail IS NULL OR CompanyEmail = '' THEN ? ELSE CompanyEmail END
                                WHERE (SupplierName = ? OR CompanyWebsite = ?)
                            """, (
                                prepared['products'][:1000],
                                prepared['category'][:200],
                                prepared['description'][:2000],
                                prepared['email'][:200],
                                prepared['name'][:255],
                                prepared['website'][:500]
                            ))
                            total_updated += 1
                        else:
                            total_skipped += 1
                    else:
                        # Insert new
                        cursor.execute("""
                            INSERT INTO FoodXSuppliers (
                                SupplierName, CompanyWebsite, CompanyEmail, 
                                Country, Products, ProductCategory, Description
                            ) VALUES (?, ?, ?, ?, ?, ?, ?)
                        """, (
                            prepared['name'][:255],
                            prepared['website'][:500],
                            prepared['email'][:200],
                            prepared['country'][:100],
                            prepared['products'][:1000],
                            prepared['category'][:200],
                            prepared['description'][:2000]
                        ))
                        total_imported += 1
                        
                        # Add to existing set
                        existing[name_lower] = True
                        existing[website_lower] = True
                    
                except Exception as e:
                    print(f"Error processing supplier {supplier.get('company', {}).get('name', 'Unknown')}: {e}")
                    continue
            
            # Commit batch
            conn.commit()
            print(f"Batch {i//batch_size + 1}: Imported {total_imported}, Updated {total_updated}, Skipped {total_skipped}")
        
        cursor.close()
        conn.close()
        
        print(f"\nImport completed:")
        print(f"  New suppliers imported: {total_imported:,}")
        print(f"  Existing suppliers updated: {total_updated:,}")
        print(f"  Skipped (no products): {total_skipped:,}")
        
        return total_imported + total_updated
        
    except Exception as e:
        print(f"Error importing suppliers: {e}")
        return 0

def create_supplier_products_table():
    """Create SupplierProducts table for individual products"""
    try:
        conn = get_connection()
        cursor = conn.cursor()
        
        # Check if table exists
        cursor.execute("""
            SELECT COUNT(*) 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME = 'SupplierProducts'
        """)
        
        if cursor.fetchone()[0] == 0:
            print("\nCreating SupplierProducts table...")
            cursor.execute("""
                CREATE TABLE SupplierProducts (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    SupplierId INT,
                    ProductName NVARCHAR(500),
                    Category NVARCHAR(200),
                    Brand NVARCHAR(200),
                    SearchText NVARCHAR(MAX),
                    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
                )
            """)
            
            # Add index
            cursor.execute("""
                CREATE INDEX IX_SupplierProducts_SupplierId 
                ON SupplierProducts(SupplierId)
            """)
            
            cursor.execute("""
                CREATE INDEX IX_SupplierProducts_ProductName 
                ON SupplierProducts(ProductName)
            """)
            
            cursor.execute("""
                CREATE INDEX IX_SupplierProducts_Category 
                ON SupplierProducts(Category)
            """)
            
            conn.commit()
            print("SupplierProducts table created successfully")
        else:
            print("\nSupplierProducts table already exists")
        
        cursor.close()
        conn.close()
        return True
        
    except Exception as e:
        print(f"Error creating SupplierProducts table: {e}")
        return False

def main():
    """Main import process"""
    json_file = r"G:\My Drive\Business intelligence\suppliers_ai_optimized.json"
    
    print("=" * 80)
    print("IMPORTING SUPPLIERS FROM AI-OPTIMIZED FILE")
    print("=" * 80)
    
    # Step 1: Load JSON data
    print("\n1. Loading AI-optimized suppliers...")
    suppliers = load_ai_optimized_suppliers(json_file)
    if not suppliers:
        print("No suppliers found in JSON file")
        return False
    
    # Step 2: Get existing suppliers
    print("\n2. Checking existing suppliers in database...")
    existing_suppliers = get_existing_suppliers()
    
    # Step 3: Import suppliers
    print("\n3. Importing suppliers to database...")
    imported_count = import_suppliers_batch(suppliers, existing_suppliers)
    
    # Step 4: Create SupplierProducts table
    print("\n4. Setting up SupplierProducts table...")
    create_supplier_products_table()
    
    print("\n" + "=" * 80)
    print(f"Import process completed!")
    print(f"Total suppliers processed: {len(suppliers):,}")
    print(f"Successfully imported/updated: {imported_count:,}")
    print("=" * 80)
    
    return imported_count > 0

if __name__ == "__main__":
    success = main()
    sys.exit(0 if success else 1)
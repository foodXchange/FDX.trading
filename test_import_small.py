#!/usr/bin/env python
"""
Test import with small batch from suppliers_ai_optimized.json
"""
import json
from db_connection import get_connection

def test_import():
    """Test import with first 10 suppliers"""
    try:
        # Load JSON
        print("Loading suppliers_ai_optimized.json...")
        with open(r"G:\My Drive\Business intelligence\suppliers_ai_optimized.json", 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        metadata = data.get('metadata', {})
        suppliers = data.get('suppliers', [])[:10]  # Only first 10
        
        print(f"Metadata: {metadata}")
        print(f"Testing with {len(suppliers)} suppliers")
        
        # Connect to database
        conn = get_connection()
        cursor = conn.cursor()
        
        # Check current count
        cursor.execute("SELECT COUNT(*) FROM FoodXSuppliers")
        current_count = cursor.fetchone()[0]
        print(f"\nCurrent suppliers in database: {current_count:,}")
        
        # Import first supplier as test
        if suppliers:
            supplier = suppliers[0]
            company = supplier.get('company', {})
            products = supplier.get('products', [])
            
            print(f"\nTest supplier: {company.get('name')}")
            print(f"  Products: {len(products)}")
            print(f"  First 3 products: {products[:3]}")
            
            # Try to insert
            products_str = "; ".join(products[:20])  # First 20 products
            
            cursor.execute("""
                SELECT COUNT(*) FROM FoodXSuppliers 
                WHERE SupplierName = ? OR CompanyWebsite = ?
            """, (company.get('name', ''), company.get('website', '')))
            
            exists = cursor.fetchone()[0] > 0
            
            if exists:
                print(f"  Supplier already exists, updating...")
                cursor.execute("""
                    UPDATE FoodXSuppliers 
                    SET Products = ?, 
                        ProductCategory = ?
                    WHERE SupplierName = ? OR CompanyWebsite = ?
                """, (
                    products_str[:1000],
                    " - ".join(supplier.get('category', []))[:200],
                    company.get('name', ''),
                    company.get('website', '')
                ))
            else:
                print(f"  Inserting new supplier...")
                cursor.execute("""
                    INSERT INTO FoodXSuppliers (
                        SupplierName, CompanyWebsite, CompanyEmail, 
                        Country, Products, ProductCategory
                    ) VALUES (?, ?, ?, ?, ?, ?)
                """, (
                    company.get('name', '')[:255],
                    company.get('website', '')[:500],
                    company.get('email', '')[:200],
                    company.get('country', '')[:100],
                    products_str[:1000],
                    " - ".join(supplier.get('category', []))[:200]
                ))
            
            conn.commit()
            print("  Success!")
        
        cursor.close()
        conn.close()
        print("\nTest completed successfully!")
        
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    test_import()
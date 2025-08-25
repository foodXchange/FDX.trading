#!/usr/bin/env python
"""
Populate SupplierProducts table with parsed individual products
from suppliers_ai_optimized.json
"""
import json
from db_connection import get_connection

def detect_certifications(product_name, supplier_name="", country=""):
    """Detect certifications from product name and supplier info"""
    product_lower = product_name.lower()
    supplier_lower = supplier_name.lower()
    
    # Detect Kosher
    is_kosher = any(term in product_lower or term in supplier_lower for term in 
                    ['kosher', 'ou certified', 'star-k', 'ok certified'])
    
    # Israeli suppliers often have kosher products
    if country == "Israel":
        is_kosher = True
    
    # Detect Halal
    is_halal = any(term in product_lower or term in supplier_lower for term in 
                   ['halal', 'muslim', 'islamic'])
    
    # Detect Organic
    is_organic = any(term in product_lower for term in 
                     ['organic', 'bio', 'ecological'])
    
    # Detect Gluten-Free
    is_gluten_free = any(term in product_lower for term in 
                         ['gluten free', 'gluten-free', 'no gluten', 'gf'])
    
    return is_kosher, is_halal, is_organic, is_gluten_free

def populate_supplier_products():
    """Populate SupplierProducts table from AI-optimized JSON"""
    try:
        # Load JSON
        print("Loading suppliers_ai_optimized.json...")
        with open(r"G:\My Drive\Business intelligence\suppliers_ai_optimized.json", 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        suppliers = data.get('suppliers', [])
        print(f"Loaded {len(suppliers):,} suppliers")
        
        conn = get_connection()
        cursor = conn.cursor()
        
        # Clear existing data
        print("Clearing existing SupplierProducts...")
        cursor.execute("TRUNCATE TABLE SupplierProducts")
        
        total_products = 0
        batch_size = 1000
        insert_batch = []
        
        print("Processing suppliers and extracting products...")
        
        for idx, supplier in enumerate(suppliers):
            company = supplier.get('company', {})
            supplier_name = company.get('name', '')
            country = company.get('country', '')
            products = supplier.get('products', [])
            categories = supplier.get('category', [])
            brands = supplier.get('brands', [])
            
            # Get primary category
            primary_category = categories[0] if categories else "General"
            primary_brand = brands[0] if brands else ""
            
            # Process each product
            for product in products[:20]:  # Limit to 20 products per supplier
                if len(product) < 3:  # Skip very short product names
                    continue
                    
                # Detect certifications
                is_kosher, is_halal, is_organic, is_gluten_free = detect_certifications(
                    product, supplier_name, country
                )
                
                # Create search text
                search_text = f"{supplier_name} {product} {primary_category} {country}"
                
                insert_batch.append((
                    idx + 1,  # SupplierId (using index as ID)
                    supplier_name[:255],
                    product[:500],
                    primary_category[:200],
                    primary_brand[:200],
                    country[:100],
                    is_kosher,
                    is_halal,
                    is_organic,
                    is_gluten_free,
                    search_text[:2000]
                ))
                
                total_products += 1
                
                # Insert batch when it reaches batch_size
                if len(insert_batch) >= batch_size:
                    cursor.executemany("""
                        INSERT INTO SupplierProducts (
                            SupplierId, SupplierName, ProductName, Category, Brand, Country,
                            IsKosher, IsHalal, IsOrganic, IsGlutenFree, SearchText
                        ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
                    """, insert_batch)
                    conn.commit()
                    print(f"  Inserted {total_products:,} products...")
                    insert_batch = []
            
            if (idx + 1) % 100 == 0:
                print(f"Processed {idx + 1:,}/{len(suppliers):,} suppliers...")
        
        # Insert remaining batch
        if insert_batch:
            cursor.executemany("""
                INSERT INTO SupplierProducts (
                    SupplierId, SupplierName, ProductName, Category, Brand, Country,
                    IsKosher, IsHalal, IsOrganic, IsGlutenFree, SearchText
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            """, insert_batch)
            conn.commit()
        
        print(f"\nTotal products inserted: {total_products:,}")
        
        # Get statistics
        cursor.execute("""
            SELECT 
                COUNT(*) as Total,
                SUM(CAST(IsKosher as INT)) as Kosher,
                SUM(CAST(IsHalal as INT)) as Halal,
                SUM(CAST(IsOrganic as INT)) as Organic,
                SUM(CAST(IsGlutenFree as INT)) as GlutenFree,
                COUNT(DISTINCT SupplierName) as UniqueSuppliers,
                COUNT(DISTINCT Category) as UniqueCategories
            FROM SupplierProducts
        """)
        
        stats = cursor.fetchone()
        if stats:
            print("\nSupplierProducts Statistics:")
            print(f"  Total products: {stats[0]:,}" if stats[0] else "  Total products: 0")
            print(f"  Kosher products: {stats[1]:,}" if stats[1] else "  Kosher products: 0")
            print(f"  Halal products: {stats[2]:,}" if stats[2] else "  Halal products: 0")
            print(f"  Organic products: {stats[3]:,}" if stats[3] else "  Organic products: 0")
            print(f"  Gluten-free products: {stats[4]:,}" if stats[4] else "  Gluten-free products: 0")
            print(f"  Unique suppliers: {stats[5]:,}" if stats[5] else "  Unique suppliers: 0")
            print(f"  Unique categories: {stats[6]:,}" if stats[6] else "  Unique categories: 0")
        
        # Sample kosher products for testing
        print("\nSample Kosher Products:")
        cursor.execute("""
            SELECT TOP 5 SupplierName, ProductName, Country
            FROM SupplierProducts
            WHERE IsKosher = 1
            ORDER BY NEWID()
        """)
        
        for row in cursor.fetchall():
            print(f"  {row[0][:40]:40} | {row[1][:40]:40} | {row[2]}")
        
        cursor.close()
        conn.close()
        
        print("\nPopulation completed successfully!")
        
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    populate_supplier_products()
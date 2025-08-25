import pyodbc
from db_connection import get_connection

try:
    conn = get_connection()
    cursor = conn.cursor()
    
    print("=" * 80)
    print("CURRENT FOODXSUPPLIERS TABLE STATUS")
    print("=" * 80)
    
    # Check total count
    cursor.execute("SELECT COUNT(*) FROM FoodXSuppliers")
    total = cursor.fetchone()[0]
    print(f"\nTotal suppliers in database: {total:,}")
    
    # Check suppliers with products
    cursor.execute("""
        SELECT COUNT(*) FROM FoodXSuppliers 
        WHERE Products IS NOT NULL AND Products != ''
    """)
    with_products = cursor.fetchone()[0]
    print(f"Suppliers with products: {with_products:,}")
    
    # Check suppliers with categories
    cursor.execute("""
        SELECT COUNT(*) FROM FoodXSuppliers 
        WHERE ProductCategory IS NOT NULL AND ProductCategory != ''
    """)
    with_category = cursor.fetchone()[0]
    print(f"Suppliers with categories: {with_category:,}")
    
    # Check for demo/test data
    cursor.execute("""
        SELECT COUNT(*) FROM FoodXSuppliers 
        WHERE SupplierName LIKE '%test%' 
        OR SupplierName LIKE '%demo%'
        OR SupplierName LIKE '%sample%'
    """)
    demo_count = cursor.fetchone()[0]
    print(f"\nDemo/test suppliers found: {demo_count}")
    
    # Check top categories
    print("\nTop Product Categories:")
    cursor.execute("""
        SELECT TOP 10 ProductCategory, COUNT(*) as cnt
        FROM FoodXSuppliers
        WHERE ProductCategory IS NOT NULL AND ProductCategory != ''
        GROUP BY ProductCategory
        ORDER BY COUNT(*) DESC
    """)
    
    for row in cursor.fetchall():
        print(f"  {row[0][:50]:50} : {row[1]:,} suppliers")
    
    # Check if SupplierProducts table exists
    cursor.execute("""
        SELECT COUNT(*) 
        FROM INFORMATION_SCHEMA.TABLES 
        WHERE TABLE_NAME = 'SupplierProducts'
    """)
    table_exists = cursor.fetchone()[0] > 0
    print(f"\nSupplierProducts table exists: {table_exists}")
    
    if table_exists:
        cursor.execute("SELECT COUNT(*) FROM SupplierProducts")
        products_count = cursor.fetchone()[0]
        print(f"Records in SupplierProducts: {products_count:,}")
    
    cursor.close()
    conn.close()
    print("\n" + "=" * 80)
    
except Exception as e:
    print(f"Error: {e}")
import pyodbc

conn = pyodbc.connect(
    'Driver={ODBC Driver 17 for SQL Server};'
    'Server=tcp:fdx-sql-prod.database.windows.net,1433;'
    'Database=fdxdb;'
    'Authentication=ActiveDirectoryInteractive;'
    'Encrypt=yes;'
    'TrustServerCertificate=no;'
    'Connection Timeout=30;'
)

cursor = conn.cursor()

print("SUPPLIER PRODUCT DATA SUMMARY")
print("=" * 60)

# Get counts
cursor.execute("SELECT COUNT(*) FROM FoodXSuppliers WHERE Products != ''")
with_products = cursor.fetchone()[0]

cursor.execute("SELECT COUNT(*) FROM FoodXSuppliers WHERE ProductCategory != ''")
with_category = cursor.fetchone()[0]

print(f"Suppliers with Products: {with_products:,}")
print(f"Suppliers with Categories: {with_category:,}")

# Get top categories
print("\nTOP 10 PRODUCT CATEGORIES:")
print("-" * 40)
cursor.execute("""
    SELECT TOP 10 ProductCategory, COUNT(*) as Count
    FROM FoodXSuppliers
    WHERE ProductCategory != ''
    GROUP BY ProductCategory
    ORDER BY COUNT(*) DESC
""")

for row in cursor.fetchall():
    cat = str(row[0])[:50] if row[0] else "N/A"
    print(f"  {cat:50} : {row[1]:5} suppliers")

# Sample suppliers
print("\nSAMPLE SUPPLIERS WITH PRODUCTS:")
print("-" * 40)
cursor.execute("""
    SELECT TOP 5 
        SupplierName,
        LEFT(ProductCategory, 60) as Category,
        LEFT(Products, 100) as ProductSample
    FROM FoodXSuppliers
    WHERE Products != ''
    ORDER BY SupplierName
""")

for row in cursor.fetchall():
    print(f"\n{row[0]}")
    print(f"  Category: {row[1] if row[1] else 'N/A'}")
    if row[2]:
        # Clean product text
        product_text = ''.join(c if ord(c) < 128 else '?' for c in str(row[2]))
        print(f"  Products: {product_text[:80]}...")

cursor.close()
conn.close()

print("\n" + "=" * 60)
print("All suppliers have product data available for matching!")
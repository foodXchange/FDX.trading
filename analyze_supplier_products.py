import pyodbc
import json
from collections import Counter

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
print("SUPPLIER PRODUCTS ANALYSIS")
print("=" * 80)

conn = get_connection()
cursor = conn.cursor()

# 1. Check product-related columns in FoodXSuppliers table
print("\n1. PRODUCT-RELATED COLUMNS IN FoodXSuppliers TABLE:")
print("-" * 50)

cursor.execute("""
    SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'FoodXSuppliers'
    AND (COLUMN_NAME LIKE '%Product%' 
         OR COLUMN_NAME LIKE '%Category%'
         OR COLUMN_NAME = 'Description')
    ORDER BY ORDINAL_POSITION
""")

columns = cursor.fetchall()
for col in columns:
    print(f"  {col[0]:25} | {col[1]:15} | Max Length: {col[2] if col[2] else 'MAX'}")

# 2. Analyze Products column content
print("\n2. ANALYZING 'Products' COLUMN:")
print("-" * 50)

cursor.execute("""
    SELECT COUNT(*) as Total,
           COUNT(CASE WHEN Products IS NOT NULL AND Products != '' THEN 1 END) as WithProducts,
           COUNT(CASE WHEN Products IS NULL OR Products = '' THEN 1 END) as WithoutProducts
    FROM FoodXSuppliers
""")

stats = cursor.fetchone()
print(f"  Total suppliers: {stats[0]:,}")
print(f"  With products: {stats[1]:,} ({stats[1]*100//stats[0]}%)")
print(f"  Without products: {stats[2]:,} ({stats[2]*100//stats[0]}%)")

# 3. Sample suppliers with products
print("\n3. SAMPLE SUPPLIERS WITH PRODUCTS:")
print("-" * 50)

cursor.execute("""
    SELECT TOP 10
        SupplierName,
        ProductCategory,
        SUBSTRING(Products, 1, 200) as ProductsSample,
        SUBSTRING(Description, 1, 150) as DescriptionSample
    FROM FoodXSuppliers
    WHERE Products IS NOT NULL AND Products != ''
    ORDER BY SupplierName
""")

for row in cursor.fetchall():
    print(f"\nSupplier: {row[0]}")
    print(f"  Category: {row[1] if row[1] else 'N/A'}")
    print(f"  Products: {row[2][:100] if row[2] else 'N/A'}...")
    print(f"  Description: {row[3][:100] if row[3] else 'N/A'}...")

# 4. Product categories analysis
print("\n4. TOP PRODUCT CATEGORIES:")
print("-" * 50)

cursor.execute("""
    SELECT TOP 20
        ProductCategory,
        COUNT(*) as SupplierCount
    FROM FoodXSuppliers
    WHERE ProductCategory IS NOT NULL AND ProductCategory != ''
    GROUP BY ProductCategory
    ORDER BY COUNT(*) DESC
""")

categories = cursor.fetchall()
for cat in categories:
    print(f"  {cat[0][:50]:50} : {cat[1]:5} suppliers")

# 5. Search for specific product keywords
print("\n5. PRODUCT KEYWORD FREQUENCY:")
print("-" * 50)

# Get a sample of product descriptions to analyze
cursor.execute("""
    SELECT Products + ' ' + ISNULL(Description, '') + ' ' + ISNULL(ProductCategory, '') as AllProductText
    FROM FoodXSuppliers
    WHERE Products IS NOT NULL AND Products != ''
""")

# Common food product keywords to search for
keywords = ['meat', 'dairy', 'fruit', 'vegetable', 'seafood', 'beverage', 'snack', 
            'grain', 'oil', 'spice', 'organic', 'frozen', 'fresh', 'canned', 'bakery',
            'pasta', 'rice', 'sauce', 'coffee', 'tea']

keyword_counts = {kw: 0 for kw in keywords}

rows = cursor.fetchall()
for row in rows:
    text = (row[0] or '').lower()
    for keyword in keywords:
        if keyword in text:
            keyword_counts[keyword] += 1

# Sort by frequency
sorted_keywords = sorted(keyword_counts.items(), key=lambda x: x[1], reverse=True)
print("  Top product keywords found in supplier data:")
for keyword, count in sorted_keywords[:15]:
    if count > 0:
        print(f"    {keyword:15} : {count:5} suppliers ({count*100//len(rows)}%)")

# 6. Suppliers by specific products
print("\n6. FIND SUPPLIERS BY PRODUCT TYPE:")
print("-" * 50)

# Example: Find meat suppliers
print("\n  Example: Meat suppliers")
cursor.execute("""
    SELECT TOP 5
        SupplierName,
        Country,
        SUBSTRING(Products, 1, 150) as ProductInfo
    FROM FoodXSuppliers
    WHERE (Products LIKE '%meat%' 
           OR Description LIKE '%meat%'
           OR ProductCategory LIKE '%meat%')
    ORDER BY SupplierName
""")

for row in cursor.fetchall():
    print(f"    - {row[0][:40]:40} | {(row[1] or 'N/A')[:20]:20}")
    if row[2]:
        print(f"      Products: {row[2][:80]}...")

# 7. Product data completeness by country
print("\n7. PRODUCT DATA COMPLETENESS BY COUNTRY:")
print("-" * 50)

cursor.execute("""
    SELECT TOP 10
        ISNULL(Country, 'Not Specified') as Country,
        COUNT(*) as TotalSuppliers,
        COUNT(CASE WHEN Products IS NOT NULL AND Products != '' THEN 1 END) as WithProducts,
        COUNT(CASE WHEN ProductCategory IS NOT NULL AND ProductCategory != '' THEN 1 END) as WithCategory
    FROM FoodXSuppliers
    GROUP BY Country
    HAVING COUNT(*) > 10
    ORDER BY COUNT(*) DESC
""")

print(f"  {'Country':30} | {'Total':8} | {'w/Products':10} | {'w/Category':10}")
print("  " + "-" * 65)
for row in cursor.fetchall():
    print(f"  {row[0][:30]:30} | {row[1]:8} | {row[2]:10} | {row[3]:10}")

# 8. Create a product index for better searchability
print("\n8. CREATING PRODUCT SEARCH CAPABILITIES:")
print("-" * 50)

# Check if we can create a full-text index
cursor.execute("""
    SELECT COUNT(*) 
    FROM FoodXSuppliers 
    WHERE Products IS NOT NULL 
    AND LEN(Products) > 10
""")
searchable = cursor.fetchone()[0]

print(f"  Suppliers with searchable product data: {searchable:,}")
print(f"  Ready for product-based matching and search")

cursor.close()
conn.close()

print("\n" + "=" * 80)
print("ANALYSIS COMPLETE")
print("=" * 80)
print("\nKey Findings:")
print("- Product data is stored in 'Products' and 'ProductCategory' columns")
print("- Description column also contains product information")
print("- Data can be searched for specific product types")
print("- Ready for supplier-product matching system")
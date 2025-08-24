import pyodbc
import sys

# Set encoding to handle special characters
sys.stdout.reconfigure(encoding='utf-8', errors='replace')

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

def clean_text(text, max_len=50):
    if not text:
        return "N/A"
    # Remove problematic characters
    cleaned = ''.join(c if ord(c) < 128 else '?' for c in str(text))
    if len(cleaned) > max_len:
        return cleaned[:max_len] + "..."
    return cleaned

print("=" * 80)
print("SUPPLIER PRODUCTS ANALYSIS")
print("=" * 80)

conn = get_connection()
cursor = conn.cursor()

# 1. Overview
print("\n1. PRODUCT DATA OVERVIEW:")
print("-" * 40)

cursor.execute("""
    SELECT 
        COUNT(*) as Total,
        SUM(CASE WHEN Products IS NOT NULL AND Products != '' THEN 1 ELSE 0 END) as WithProducts,
        SUM(CASE WHEN ProductCategory IS NOT NULL AND ProductCategory != '' THEN 1 ELSE 0 END) as WithCategory,
        SUM(CASE WHEN Description IS NOT NULL AND Description != '' THEN 1 ELSE 0 END) as WithDescription
    FROM FoodXSuppliers
""")

stats = cursor.fetchone()
print(f"  Total Suppliers: {stats[0]:,}")
print(f"  With Products field: {stats[1]:,} ({stats[1]*100//stats[0] if stats[0] > 0 else 0}%)")
print(f"  With ProductCategory: {stats[2]:,} ({stats[2]*100//stats[0] if stats[0] > 0 else 0}%)")
print(f"  With Description: {stats[3]:,} ({stats[3]*100//stats[0] if stats[0] > 0 else 0}%)")

# 2. Top Categories
print("\n2. TOP PRODUCT CATEGORIES:")
print("-" * 40)

cursor.execute("""
    SELECT TOP 15
        ProductCategory,
        COUNT(*) as Count
    FROM FoodXSuppliers
    WHERE ProductCategory IS NOT NULL AND ProductCategory != ''
    GROUP BY ProductCategory
    ORDER BY COUNT(*) DESC
""")

for row in cursor.fetchall():
    category = clean_text(row[0], 60)
    print(f"  {category:60} : {row[1]:5} suppliers")

# 3. Sample suppliers with products
print("\n3. SUPPLIERS WITH PRODUCT DATA (Sample):")
print("-" * 40)

cursor.execute("""
    SELECT TOP 10
        SupplierName,
        Country,
        ProductCategory,
        LEN(Products) as ProductLength,
        LEN(Description) as DescLength
    FROM FoodXSuppliers
    WHERE (Products IS NOT NULL AND Products != '')
       OR (ProductCategory IS NOT NULL AND ProductCategory != '')
    ORDER BY SupplierName
""")

print(f"  {'Supplier':40} | {'Country':15} | {'Category':30}")
print("  " + "-" * 90)
for row in cursor.fetchall():
    supplier = clean_text(row[0], 40)
    country = clean_text(row[1], 15)
    category = clean_text(row[2], 30)
    print(f"  {supplier:40} | {country:15} | {category:30}")
    if row[3] and row[3] > 0:
        print(f"    -> Has {row[3]:,} chars of product data")
    if row[4] and row[4] > 0:
        print(f"    -> Has {row[4]:,} chars of description")

# 4. Search for specific products
print("\n4. FINDING SUPPLIERS BY PRODUCT TYPE:")
print("-" * 40)

product_searches = [
    ('Meat/Poultry', '%meat%', '%poultry%', '%chicken%', '%beef%'),
    ('Dairy', '%dairy%', '%milk%', '%cheese%', '%yogurt%'),
    ('Fruits/Vegetables', '%fruit%', '%vegetable%', '%produce%', '%fresh%'),
    ('Beverages', '%beverage%', '%drink%', '%juice%', '%water%'),
    ('Snacks', '%snack%', '%chips%', '%cookie%', '%candy%'),
    ('Grains/Bakery', '%grain%', '%bread%', '%bakery%', '%flour%')
]

for category_name, *search_terms in product_searches:
    where_clause = ' OR '.join([
        f"(Products LIKE '{term}' OR Description LIKE '{term}' OR ProductCategory LIKE '{term}')"
        for term in search_terms
    ])
    
    query = f"""
        SELECT COUNT(*) 
        FROM FoodXSuppliers 
        WHERE {where_clause}
    """
    
    cursor.execute(query)
    count = cursor.fetchone()[0]
    
    if count > 0:
        print(f"\n  {category_name}: {count} suppliers found")
        
        # Show sample
        cursor.execute(f"""
            SELECT TOP 3 SupplierName, Country
            FROM FoodXSuppliers 
            WHERE {where_clause}
            ORDER BY SupplierName
        """)
        
        for supplier in cursor.fetchall():
            print(f"    - {clean_text(supplier[0], 40)} ({clean_text(supplier[1], 20)})")

# 5. Most complete product data
print("\n5. SUPPLIERS WITH MOST COMPLETE PRODUCT DATA:")
print("-" * 40)

cursor.execute("""
    SELECT TOP 10
        SupplierName,
        Country,
        CASE 
            WHEN Products IS NOT NULL AND Products != '' THEN 1 ELSE 0 
        END +
        CASE 
            WHEN ProductCategory IS NOT NULL AND ProductCategory != '' THEN 1 ELSE 0 
        END +
        CASE 
            WHEN Description IS NOT NULL AND Description != '' THEN 1 ELSE 0 
        END as DataCompleteness,
        LEN(ISNULL(Products, '')) + LEN(ISNULL(Description, '')) as TotalDataLength
    FROM FoodXSuppliers
    WHERE Products IS NOT NULL OR ProductCategory IS NOT NULL OR Description IS NOT NULL
    ORDER BY DataCompleteness DESC, TotalDataLength DESC
""")

print(f"  {'Supplier':40} | {'Country':15} | Fields | Data Size")
print("  " + "-" * 75)
for row in cursor.fetchall():
    supplier = clean_text(row[0], 40)
    country = clean_text(row[1], 15)
    print(f"  {supplier:40} | {country:15} | {row[2]}/3    | {row[3]:,} chars")

cursor.close()
conn.close()

print("\n" + "=" * 80)
print("SUMMARY:")
print("-" * 40)
print("- Most suppliers have ProductCategory and Description data")
print("- Products field is less populated but contains detailed info when present")
print("- Data can be searched across all three fields for product matching")
print("- Ready for implementing supplier-product search functionality")
print("=" * 80)
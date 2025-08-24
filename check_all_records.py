import json
import pyodbc
import os

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
print("COMPLETE DATABASE INVENTORY CHECK")
print("=" * 80)

# Check JSON files
base_path = r"G:\My Drive\Business intelligence"
files = {
    'buyers': os.path.join(base_path, 'food_buyers_database.json'),
    'suppliers': os.path.join(base_path, 'food_suppliers_database.json'),
    'suppliers1': os.path.join(base_path, 'food_suppliers_database1.json'),
    'exhibitors': os.path.join(base_path, 'accessible_exhibitions_exhibitors.json')
}

print("\n1. JSON FILES AVAILABLE:")
print("-" * 40)
json_counts = {}

for name, path in files.items():
    if os.path.exists(path):
        size_mb = os.path.getsize(path) / 1024 / 1024
        
        # Count records in each file
        try:
            with open(path, 'r', encoding='utf-8') as f:
                data = json.load(f)
                
            if name == 'buyers':
                count = len(data.get('all_buyers', []))
                israeli = len(data.get('israeli_buyers', []))
                european = len(data.get('european_buyers', []))
                middle_east = len(data.get('middle_east_buyers', []))
                json_counts[name] = count
                print(f"  {name:12} : {count:6,} records ({size_mb:.1f} MB)")
                print(f"                  - Israeli: {israeli:,}")
                print(f"                  - European: {european:,}")
                print(f"                  - Middle East: {middle_east:,}")
                
            elif name.startswith('suppliers'):
                if isinstance(data, dict) and 'suppliers' in data:
                    count = len(data['suppliers'])
                elif isinstance(data, list):
                    count = len(data)
                else:
                    count = 0
                json_counts[name] = count
                print(f"  {name:12} : {count:6,} records ({size_mb:.1f} MB)")
                
            elif name == 'exhibitors':
                if isinstance(data, list):
                    count = len(data)
                else:
                    count = 0
                json_counts[name] = count
                print(f"  {name:12} : {count:6,} records ({size_mb:.1f} MB)")
                
        except Exception as e:
            print(f"  {name:12} : Error reading - {str(e)[:50]}")
    else:
        print(f"  {name:12} : File not found")

# Check database
print("\n2. DATABASE TABLES (FoodX):")
print("-" * 40)

conn = get_connection()
cursor = conn.cursor()

tables = [
    ('FoodXBuyers', 'SELECT COUNT(*) FROM FoodXBuyers'),
    ('FoodXSuppliers', 'SELECT COUNT(*) FROM FoodXSuppliers'),
    ('Exhibitors', 'SELECT COUNT(*) FROM Exhibitors'),
    ('Exhibitions', 'SELECT COUNT(*) FROM Exhibitions'),
    ('FoodXProducts', 'SELECT COUNT(*) FROM FoodXProducts'),
    ('Orders', 'SELECT COUNT(*) FROM Orders'),
    ('OrderItems', 'SELECT COUNT(*) FROM OrderItems')
]

db_counts = {}
for table_name, query in tables:
    try:
        cursor.execute(query)
        count = cursor.fetchone()[0]
        db_counts[table_name] = count
        print(f"  {table_name:15} : {count:6,} records")
    except:
        print(f"  {table_name:15} : Table not found")

# Check other database tables
print("\n3. OTHER DATABASE TABLES:")
print("-" * 40)

other_tables = [
    ('Users', 'SELECT COUNT(*) FROM Users'),
    ('Companies', 'SELECT COUNT(*) FROM Companies'),
    ('Buyers', 'SELECT COUNT(*) FROM Buyers'),
    ('Suppliers', 'SELECT COUNT(*) FROM Suppliers'),
    ('Products', 'SELECT COUNT(*) FROM Products'),
    ('Agents', 'SELECT COUNT(*) FROM Agents'),
    ('Experts', 'SELECT COUNT(*) FROM Experts'),
    ('AspNetUsers', 'SELECT COUNT(*) FROM AspNetUsers'),
    ('MagicLinkTokens', 'SELECT COUNT(*) FROM MagicLinkTokens')
]

for table_name, query in other_tables:
    try:
        cursor.execute(query)
        count = cursor.fetchone()[0]
        if count > 0:
            print(f"  {table_name:15} : {count:6,} records")
    except:
        pass

# Summary
print("\n" + "=" * 80)
print("IMPORT SUMMARY:")
print("=" * 80)

# Buyers
if 'buyers' in json_counts and 'FoodXBuyers' in db_counts:
    json_total = json_counts['buyers']
    db_total = db_counts['FoodXBuyers']
    percent = (db_total / json_total * 100) if json_total > 0 else 0
    print(f"\nBuyers:")
    print(f"  JSON file has  : {json_total:,} records")
    print(f"  Database has   : {db_total:,} records")
    print(f"  Import rate    : {percent:.1f}%")
    if db_total < json_total:
        print(f"  Missing        : {json_total - db_total:,} records")

# Suppliers
total_json_suppliers = json_counts.get('suppliers', 0) + json_counts.get('suppliers1', 0)
if total_json_suppliers > 0 and 'FoodXSuppliers' in db_counts:
    db_total = db_counts['FoodXSuppliers']
    percent = (db_total / total_json_suppliers * 100) if total_json_suppliers > 0 else 0
    print(f"\nSuppliers:")
    print(f"  JSON files have: {total_json_suppliers:,} records")
    print(f"    - Main file  : {json_counts.get('suppliers', 0):,}")
    print(f"    - File 1     : {json_counts.get('suppliers1', 0):,}")
    print(f"  Database has   : {db_total:,} records")
    print(f"  Import rate    : {percent:.1f}%")
    if db_total < total_json_suppliers:
        print(f"  Missing        : {total_json_suppliers - db_total:,} records")

# Exhibitors
if 'exhibitors' in json_counts and 'Exhibitors' in db_counts:
    json_total = json_counts['exhibitors']
    db_total = db_counts['Exhibitors']
    percent = (db_total / json_total * 100) if json_total > 0 else 0
    print(f"\nExhibitors:")
    print(f"  JSON file has  : {json_total:,} records")
    print(f"  Database has   : {db_total:,} records")
    print(f"  Import rate    : {percent:.1f}%")
    if db_total < json_total:
        print(f"  Missing        : {json_total - db_total:,} records NOT IMPORTED")

print("\n" + "=" * 80)
print("TOTAL BUSINESS ENTITIES IN DATABASE:")
print("=" * 80)
total_entities = db_counts.get('FoodXBuyers', 0) + db_counts.get('FoodXSuppliers', 0) + db_counts.get('Exhibitors', 0)
print(f"  Total: {total_entities:,} business entities")

cursor.close()
conn.close()
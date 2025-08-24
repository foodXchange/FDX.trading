import json
import pyodbc
from datetime import datetime

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

print("Importing buyers...")
file_path = r"G:\My Drive\Business intelligence\food_buyers_database.json"

with open(file_path, 'r', encoding='utf-8') as f:
    data = json.load(f)

conn = get_connection()
cursor = conn.cursor()

buyers = data.get('all_buyers', [])
print(f"Found {len(buyers)} buyers in JSON file")

imported = 0
for i, buyer in enumerate(buyers):
    try:
        company = buyer.get('Company', '')
        if not company:
            continue
            
        cursor.execute("""
            INSERT INTO FoodXBuyers (
                Company, Type, Website, Categories, Size, 
                Stores, Region, Markets, Domain,
                ProcurementEmail, ProcurementPhone, ProcurementManager,
                CertificationsRequired, PaymentTerms,
                GeneralEmail, GeneralPhone, CreatedAt
            ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        """, (
            company[:200],
            (buyer.get('Type') or '')[:100],
            (buyer.get('Website') or '')[:500],
            (buyer.get('Categories') or '')[:200],
            (buyer.get('Size') or '')[:50],
            (buyer.get('Stores') or '')[:100],
            (buyer.get('Region') or '')[:100],
            (buyer.get('Markets') or '')[:200],
            (buyer.get('Domain') or '')[:200],
            (buyer.get('Procurement Email') or '')[:200],
            (buyer.get('Procurement Phone') or '')[:50],
            (buyer.get('Procurement Manager') or '')[:200],
            buyer.get('Certifications Required') or '',
            (buyer.get('Payment Terms') or '')[:200],
            (buyer.get('General Emails') or '')[:200],
            (buyer.get('General Phones') or '')[:50],
            datetime.now()
        ))
        imported += 1
        
        if imported % 50 == 0:
            print(f"Imported {imported} buyers...")
            conn.commit()
            
    except Exception as e:
        print(f"Error on buyer {i}: {str(e)[:100]}")
        continue

conn.commit()
print(f"\n[SUCCESS] Imported {imported} buyers")

# Verify
cursor.execute("SELECT COUNT(*) FROM FoodXBuyers")
count = cursor.fetchone()[0]
print(f"Total buyers in database: {count}")

cursor.close()
conn.close()
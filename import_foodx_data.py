import json
import pyodbc
import os
from datetime import datetime

def get_connection():
    """Get database connection using Windows Authentication"""
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

def import_buyers(file_path, conn):
    """Import buyers from JSON file"""
    print(f"\nImporting buyers from {file_path}...")
    
    with open(file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    cursor = conn.cursor()
    buyers = data.get('all_buyers', [])
    imported = 0
    errors = 0
    
    for buyer in buyers:
        try:
            # Get values from JSON, handling None values
            company = buyer.get('Company', '')
            if not company:  # Skip if no company name
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
                company[:200],  # Truncate to fit column size
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
            
            if imported % 100 == 0:
                print(f"  Processed {imported} buyers...")
                conn.commit()
                
        except Exception as e:
            errors += 1
            if errors <= 5:  # Only show first 5 errors
                print(f"  Error importing buyer {company[:50]}: {str(e)[:100]}")
    
    conn.commit()
    print(f"  [OK] Imported {imported} buyers ({errors} errors)")
    return imported

def import_suppliers(file_path, conn):
    """Import suppliers from JSON file"""
    print(f"\nImporting suppliers from {file_path}...")
    
    with open(file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    cursor = conn.cursor()
    suppliers = data.get('suppliers', [])
    imported = 0
    errors = 0
    
    for supplier in suppliers:
        try:
            # Get supplier name from multiple possible fields
            supplier_name = (supplier.get('Supplier Name') or 
                           supplier.get('Company Name') or 
                           supplier.get('CompanyName') or '')
            
            if not supplier_name:  # Skip if no name
                continue
                
            cursor.execute("""
                INSERT INTO FoodXSuppliers (
                    SupplierName, CompanyWebsite, Description,
                    ProductCategory, Address, CompanyEmail,
                    Phone, Products, Country, PaymentTerms,
                    Incoterms, CreatedAt
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            """, (
                supplier_name[:200],
                (supplier.get('Company Website') or supplier.get('Website') or '')[:500],
                (supplier.get('Description') or '')[:4000],  # NVARCHAR(MAX) but limit for safety
                (supplier.get('Product Category') or supplier.get('Category') or '')[:200],
                (supplier.get('Address') or supplier.get('Company Address') or '')[:500],
                (supplier.get('Company Email') or supplier.get('Email') or '')[:200],
                (supplier.get('Phone') or supplier.get('Tel') or '')[:50],
                (supplier.get('Products') or '')[:4000],
                (supplier.get('Country') or '')[:100],
                (supplier.get('Terms of Payment') or supplier.get('Payment Terms') or '')[:200],
                (supplier.get('Incoterms') or '')[:100],
                datetime.now()
            ))
            imported += 1
            
            if imported % 100 == 0:
                print(f"  Processed {imported} suppliers...")
                conn.commit()
                
        except Exception as e:
            errors += 1
            if errors <= 5:  # Only show first 5 errors
                print(f"  Error importing supplier {supplier_name[:50]}: {str(e)[:100]}")
    
    conn.commit()
    print(f"  [OK] Imported {imported} suppliers ({errors} errors)")
    return imported

def import_exhibitors(file_path, conn):
    """Import exhibitors from JSON file"""
    print(f"\nImporting exhibitors from {file_path}...")
    
    with open(file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    cursor = conn.cursor()
    imported_exhibitors = 0
    errors = 0
    exhibitions = {}
    
    for exhibitor in data:
        try:
            exhibition_name = exhibitor.get('Exhibition', '')
            
            # Get or create exhibition
            if exhibition_name and exhibition_name not in exhibitions:
                cursor.execute("SELECT Id FROM Exhibitions WHERE Name = ?", (exhibition_name,))
                result = cursor.fetchone()
                
                if result:
                    exhibitions[exhibition_name] = result[0]
                else:
                    cursor.execute("""
                        INSERT INTO Exhibitions (Name, SourceUrl, CreatedAt) 
                        VALUES (?, ?, ?)
                    """, (
                        exhibition_name[:200],
                        (exhibitor.get('source_url') or '')[:500],
                        datetime.now()
                    ))
                    cursor.execute("SELECT @@IDENTITY")
                    exhibitions[exhibition_name] = cursor.fetchone()[0]
            
            # Insert exhibitor
            company_name = exhibitor.get('Company Name', '')
            if not company_name:
                continue
                
            cursor.execute("""
                INSERT INTO Exhibitors (
                    CompanyName, ProfileUrl, Country, Products, 
                    Contact, ExhibitionId, CreatedAt
                ) VALUES (?, ?, ?, ?, ?, ?, ?)
            """, (
                company_name[:200],
                (exhibitor.get('Profile URL') or '')[:500],
                (exhibitor.get('Country') or '')[:100],
                (exhibitor.get('Products') or '')[:4000],
                (exhibitor.get('Contact') or '')[:500],
                exhibitions.get(exhibition_name) if exhibition_name else None,
                datetime.now()
            ))
            imported_exhibitors += 1
            
            if imported_exhibitors % 100 == 0:
                print(f"  Processed {imported_exhibitors} exhibitors...")
                conn.commit()
                
        except Exception as e:
            errors += 1
            if errors <= 5:
                print(f"  Error importing exhibitor: {str(e)[:100]}")
    
    conn.commit()
    print(f"  [OK] Imported {imported_exhibitors} exhibitors ({errors} errors)")
    print(f"  [OK] Created/found {len(exhibitions)} exhibitions")
    return imported_exhibitors

def verify_import(conn):
    """Verify the imported data"""
    print("\n" + "=" * 80)
    print("VERIFICATION")
    print("=" * 80)
    
    cursor = conn.cursor()
    
    tables = [
        ('FoodXBuyers', 'Company'),
        ('FoodXSuppliers', 'SupplierName'),
        ('Exhibitors', 'CompanyName'),
        ('Exhibitions', 'Name')
    ]
    
    for table, name_col in tables:
        # Get count
        cursor.execute(f"SELECT COUNT(*) FROM {table}")
        count = cursor.fetchone()[0]
        
        # Get sample records
        cursor.execute(f"SELECT TOP 3 {name_col} FROM {table} ORDER BY Id")
        samples = cursor.fetchall()
        
        print(f"\n{table}: {count} records")
        if samples:
            print("  Sample records:")
            for sample in samples:
                print(f"    - {sample[0][:60]}...")
    
    cursor.close()

def main():
    print("=" * 80)
    print("FOODX DATA IMPORT TOOL")
    print("=" * 80)
    
    base_path = r"G:\My Drive\Business intelligence"
    
    files = {
        'buyers': os.path.join(base_path, 'food_buyers_database.json'),
        'suppliers': os.path.join(base_path, 'food_suppliers_database.json'),
        'exhibitors': os.path.join(base_path, 'accessible_exhibitions_exhibitors.json')
    }
    
    # Check if files exist
    print("\nChecking files...")
    for name, path in files.items():
        if os.path.exists(path):
            size = os.path.getsize(path) / 1024 / 1024  # MB
            print(f"  [OK] {name}: {path} ({size:.2f} MB)")
        else:
            print(f"  [X] {name}: {path} NOT FOUND")
            return
    
    try:
        # Connect to database
        print("\nConnecting to database...")
        conn = get_connection()
        print("  [OK] Connected to Azure SQL Database")
        
        # Import data
        total_imported = 0
        
        # Import buyers
        imported = import_buyers(files['buyers'], conn)
        total_imported += imported
        
        # Import suppliers
        imported = import_suppliers(files['suppliers'], conn)
        total_imported += imported
        
        # Import exhibitors
        imported = import_exhibitors(files['exhibitors'], conn)
        total_imported += imported
        
        # Verify import
        verify_import(conn)
        
        print("\n" + "=" * 80)
        print(f"IMPORT COMPLETE: {total_imported} total records imported")
        print("=" * 80)
        
        conn.close()
        
    except Exception as e:
        print(f"\n[ERROR] {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    main()
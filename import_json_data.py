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
    
    # Clear existing Buyers table (from the 5 seed records)
    cursor.execute("DELETE FROM Buyers")
    print(f"Cleared existing buyers table")
    
    buyers = data.get('all_buyers', [])
    imported = 0
    
    for buyer in buyers:
        try:
            cursor.execute("""
                INSERT INTO Buyers (
                    Company, Type, Website, Categories, Size, 
                    Stores, Region, Markets, Domain,
                    ProcurementEmail, ProcurementPhone, ProcurementManager,
                    CertificationsRequired, PaymentTerms,
                    GeneralEmail, GeneralPhone, CreatedAt
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            """, (
                buyer.get('Company', ''),
                buyer.get('Type', ''),
                buyer.get('Website', ''),
                buyer.get('Categories', ''),
                buyer.get('Size', ''),
                buyer.get('Stores', ''),
                buyer.get('Region', ''),
                buyer.get('Markets', ''),
                buyer.get('Domain', ''),
                buyer.get('Procurement Email', ''),
                buyer.get('Procurement Phone', ''),
                buyer.get('Procurement Manager', ''),
                buyer.get('Certifications Required', ''),
                buyer.get('Payment Terms', ''),
                buyer.get('General Emails', ''),
                buyer.get('General Phones', ''),
                datetime.now()
            ))
            imported += 1
        except Exception as e:
            print(f"  Error importing buyer {buyer.get('Company', 'Unknown')}: {e}")
    
    conn.commit()
    print(f"  [OK] Imported {imported} buyers")
    return imported

def import_suppliers(file_path, conn):
    """Import suppliers from JSON file"""
    print(f"\nImporting suppliers from {file_path}...")
    
    with open(file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    cursor = conn.cursor()
    
    # Clear existing Suppliers table (from the 5 seed records)
    cursor.execute("DELETE FROM Suppliers")
    print(f"Cleared existing suppliers table")
    
    suppliers = data.get('suppliers', [])
    imported = 0
    
    for supplier in suppliers:
        try:
            cursor.execute("""
                INSERT INTO Suppliers (
                    SupplierName, CompanyWebsite, Description,
                    ProductCategory, Address, CompanyEmail,
                    Phone, Products, Country, PaymentTerms,
                    Incoterms, CreatedAt
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            """, (
                supplier.get('Supplier Name', supplier.get('Company Name', '')),
                supplier.get('Company Website', ''),
                supplier.get('Description', ''),
                supplier.get('Product Category', ''),
                supplier.get('Address', supplier.get('Company Address', '')),
                supplier.get('Company Email', ''),
                supplier.get('Phone', ''),
                supplier.get('Products', ''),
                supplier.get('Country', ''),
                supplier.get('Terms of Payment', ''),
                supplier.get('Incoterms', ''),
                datetime.now()
            ))
            imported += 1
        except Exception as e:
            print(f"  Error importing supplier {supplier.get('Supplier Name', 'Unknown')}: {e}")
    
    conn.commit()
    print(f"  [OK] Imported {imported} suppliers")
    return imported

def import_exhibitors(file_path, conn):
    """Import exhibitors from JSON file"""
    print(f"\nImporting exhibitors from {file_path}...")
    
    with open(file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    cursor = conn.cursor()
    
    # First, create Exhibitions table if it doesn't exist
    cursor.execute("""
        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Exhibitions' AND xtype='U')
        CREATE TABLE Exhibitions (
            Id INT IDENTITY(1,1) PRIMARY KEY,
            Name NVARCHAR(200) NOT NULL,
            SourceUrl NVARCHAR(500),
            CreatedAt DATETIME DEFAULT GETDATE()
        )
    """)
    
    # Create Exhibitors table if it doesn't exist
    cursor.execute("""
        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Exhibitors' AND xtype='U')
        CREATE TABLE Exhibitors (
            Id INT IDENTITY(1,1) PRIMARY KEY,
            CompanyName NVARCHAR(200) NOT NULL,
            ProfileUrl NVARCHAR(500),
            Country NVARCHAR(100),
            Products NVARCHAR(MAX),
            Contact NVARCHAR(500),
            ExhibitionId INT,
            CreatedAt DATETIME DEFAULT GETDATE(),
            FOREIGN KEY (ExhibitionId) REFERENCES Exhibitions(Id)
        )
    """)
    
    conn.commit()
    
    imported_exhibitors = 0
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
                        INSERT INTO Exhibitions (Name, SourceUrl) 
                        VALUES (?, ?)
                    """, (exhibition_name, exhibitor.get('source_url', '')))
                    cursor.execute("SELECT @@IDENTITY")
                    exhibitions[exhibition_name] = cursor.fetchone()[0]
            
            # Insert exhibitor
            cursor.execute("""
                INSERT INTO Exhibitors (
                    CompanyName, ProfileUrl, Country, Products, 
                    Contact, ExhibitionId
                ) VALUES (?, ?, ?, ?, ?, ?)
            """, (
                exhibitor.get('Company Name', ''),
                exhibitor.get('Profile URL', ''),
                exhibitor.get('Country', ''),
                exhibitor.get('Products', ''),
                exhibitor.get('Contact', ''),
                exhibitions.get(exhibition_name) if exhibition_name else None
            ))
            imported_exhibitors += 1
            
        except Exception as e:
            print(f"  Error importing exhibitor {exhibitor.get('Company Name', 'Unknown')}: {e}")
    
    conn.commit()
    print(f"  [OK] Imported {imported_exhibitors} exhibitors")
    print(f"  [OK] Created/found {len(exhibitions)} exhibitions")
    return imported_exhibitors

def main():
    print("=" * 80)
    print("JSON DATA IMPORT TOOL")
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
        
        # Show summary
        print("\n" + "=" * 80)
        print("IMPORT SUMMARY")
        print("=" * 80)
        
        cursor = conn.cursor()
        
        # Get counts
        cursor.execute("SELECT COUNT(*) FROM Buyers")
        buyer_count = cursor.fetchone()[0]
        
        cursor.execute("SELECT COUNT(*) FROM Suppliers")
        supplier_count = cursor.fetchone()[0]
        
        cursor.execute("SELECT COUNT(*) FROM Exhibitors")
        exhibitor_count = cursor.fetchone()[0]
        
        print(f"\nDatabase now contains:")
        print(f"  - {buyer_count} Buyers")
        print(f"  - {supplier_count} Suppliers")
        print(f"  - {exhibitor_count} Exhibitors")
        print(f"\nTotal records imported: {total_imported}")
        
        cursor.close()
        conn.close()
        
        print("\n[SUCCESS] Import completed successfully!")
        
    except Exception as e:
        print(f"\n[ERROR] Error: {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    main()
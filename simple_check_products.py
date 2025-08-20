import pyodbc
import struct

# Azure SQL Database connection
server = 'fdx-sql-prod.database.windows.net'
database = 'fdxdb'

# Connection string using Windows Authentication
connection_string = f"Driver={{ODBC Driver 18 for SQL Server}};Server={server};Database={database};Authentication=ActiveDirectoryInteractive;Encrypt=yes;TrustServerCertificate=no"

print("Connecting to database...")
with pyodbc.connect(connection_string) as conn:
    cursor = conn.cursor()
    
    print("Checking Products table...")
    print("=" * 60)
    
    # Check if table exists
    cursor.execute("""
        SELECT COUNT(*) 
        FROM INFORMATION_SCHEMA.TABLES 
        WHERE TABLE_NAME = 'Products'
    """)
    
    exists = cursor.fetchone()[0]
    
    if exists:
        print("✓ Products table EXISTS")
        cursor.execute("SELECT COUNT(*) FROM Products")
        count = cursor.fetchone()[0]
        print(f"  Rows: {count}")
        
        if count > 0:
            print("\nSample data:")
            cursor.execute("SELECT TOP 3 Name, Category, Price FROM Products")
            for row in cursor.fetchall():
                print(f"  - {row[0]} ({row[1]}): ${row[2]}")
    else:
        print("✗ Products table NOT FOUND")
        print("\nCreating Products table...")
        
        # Create the table
        create_sql = """
        CREATE TABLE Products (
            Id INT IDENTITY(1,1) PRIMARY KEY,
            Name NVARCHAR(200) NOT NULL,
            Category NVARCHAR(50),
            Description NVARCHAR(1000),
            Price DECIMAL(18,2) NOT NULL,
            Unit NVARCHAR(20) NOT NULL,
            MinOrderQuantity INT DEFAULT 1,
            StockQuantity INT DEFAULT 0,
            SKU NVARCHAR(50),
            Origin NVARCHAR(100),
            IsOrganic BIT DEFAULT 0,
            IsAvailable BIT DEFAULT 1,
            ImageUrl NVARCHAR(500),
            SupplierId INT,
            CompanyId INT,
            CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
            UpdatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
        )
        """
        
        cursor.execute(create_sql)
        conn.commit()
        print("✓ Products table created!")
        
        # Add indexes
        print("Adding indexes...")
        cursor.execute("CREATE INDEX IX_Products_Category ON Products(Category)")
        cursor.execute("CREATE INDEX IX_Products_SupplierId ON Products(SupplierId)")
        cursor.execute("CREATE INDEX IX_Products_CompanyId ON Products(CompanyId)")
        conn.commit()
        print("✓ Indexes added!")
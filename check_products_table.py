import pyodbc
import os
from azure.identity import DefaultAzureCredential

# Azure SQL Database connection
server = 'fdx-sql-prod.database.windows.net'
database = 'fdxdb'

# Get access token
credential = DefaultAzureCredential()
token = credential.get_token("https://database.windows.net/").token

# Connection string with access token
connection_string = f"Driver={{ODBC Driver 18 for SQL Server}};Server={server};Database={database};Encrypt=yes;TrustServerCertificate=no"

# Connect using token authentication
with pyodbc.connect(connection_string, attrs_before={1256: token.encode('utf-16-le')}) as conn:
    cursor = conn.cursor()
    
    print("Checking if Products table exists...")
    print("=" * 60)
    
    # Check if Products table exists
    cursor.execute("""
        SELECT TABLE_NAME
        FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_NAME = 'Products'
    """)
    
    result = cursor.fetchone()
    if result:
        print(f"✓ Products table EXISTS")
        
        # Count rows
        cursor.execute("SELECT COUNT(*) FROM Products")
        count = cursor.fetchone()[0]
        print(f"  Row count: {count}")
        
        # Show sample data
        if count > 0:
            print("\nSample Products:")
            print("-" * 60)
            cursor.execute("SELECT TOP 5 Id, Name, Category, Price, Unit FROM Products")
            for row in cursor.fetchall():
                print(f"  {row.Id}: {row.Name} ({row.Category}) - ${row.Price}/{row.Unit}")
    else:
        print("✗ Products table NOT FOUND")
        print("\nAttempting to create Products table...")
        
        # Drop if exists first (in case of partial creation)
        try:
            cursor.execute("DROP TABLE IF EXISTS Products")
            conn.commit()
        except:
            pass
        
        # Create table
        cursor.execute("""
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
        """)
        conn.commit()
        print("✓ Products table created successfully")
        
        # Add indexes
        try:
            cursor.execute("CREATE INDEX IX_Products_Category ON Products(Category)")
            cursor.execute("CREATE INDEX IX_Products_SupplierId ON Products(SupplierId)")
            cursor.execute("CREATE INDEX IX_Products_CompanyId ON Products(CompanyId)")
            conn.commit()
            print("✓ Indexes created")
        except Exception as e:
            print(f"Note: Could not create indexes: {e}")
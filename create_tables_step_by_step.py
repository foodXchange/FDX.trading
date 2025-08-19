import pyodbc
import struct
import subprocess

# Get Azure AD token
result = subprocess.run(['az', 'account', 'get-access-token', '--resource', 'https://database.windows.net/', '--query', 'accessToken', '-o', 'tsv'], 
                       capture_output=True, text=True, shell=True)
access_token = result.stdout.strip()

# Convert token for connection
token_bytes = bytes(access_token, 'utf-8')
exptoken = b''
for i in token_bytes:
    exptoken += bytes({i})
    exptoken += bytes(1)
tokenstruct = struct.pack("=i", len(exptoken)) + exptoken

# Azure SQL connection parameters
server = 'fdx-sql-prod.database.windows.net'
database = 'fdxdb'
driver = '{ODBC Driver 17 for SQL Server}'
connection_string = f'DRIVER={driver};SERVER={server};DATABASE={database}'

try:
    # Connect using access token
    conn = pyodbc.connect(connection_string, attrs_before={1256: tokenstruct})
    cursor = conn.cursor()
    
    print("Connected to Azure SQL Database")
    print("=" * 60)
    
    # Create Buyers table
    print("Creating Buyers table...")
    cursor.execute("""
        CREATE TABLE [dbo].[Buyers] (
            Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            UserId INT NOT NULL,
            CompanyId INT NULL,
            Department NVARCHAR(100) NULL,
            CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
            FOREIGN KEY (UserId) REFERENCES [dbo].[Users](Id),
            FOREIGN KEY (CompanyId) REFERENCES [dbo].[Companies](Id)
        )
    """)
    conn.commit()
    print("SUCCESS: Buyers table created")
    
    # Create Suppliers table
    print("Creating Suppliers table...")
    cursor.execute("""
        CREATE TABLE [dbo].[Suppliers] (
            Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            UserId INT NOT NULL,
            CompanyId INT NULL,
            SupplierType NVARCHAR(50) NULL,
            CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
            FOREIGN KEY (UserId) REFERENCES [dbo].[Users](Id),
            FOREIGN KEY (CompanyId) REFERENCES [dbo].[Companies](Id)
        )
    """)
    conn.commit()
    print("SUCCESS: Suppliers table created")
    
    # Create Experts table
    print("Creating Experts table...")
    cursor.execute("""
        CREATE TABLE [dbo].[Experts] (
            Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            UserId INT NOT NULL,
            Specialization NVARCHAR(200) NULL,
            CertificationNumber NVARCHAR(50) NULL,
            CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
            FOREIGN KEY (UserId) REFERENCES [dbo].[Users](Id)
        )
    """)
    conn.commit()
    print("SUCCESS: Experts table created")
    
    # Create Agents table
    print("Creating Agents table...")
    cursor.execute("""
        CREATE TABLE [dbo].[Agents] (
            Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            UserId INT NOT NULL,
            AgencyName NVARCHAR(200) NULL,
            TerritoryRegion NVARCHAR(100) NULL,
            CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
            FOREIGN KEY (UserId) REFERENCES [dbo].[Users](Id)
        )
    """)
    conn.commit()
    print("SUCCESS: Agents table created")
    
    # Create SystemAdmins table
    print("Creating SystemAdmins table...")
    cursor.execute("""
        CREATE TABLE [dbo].[SystemAdmins] (
            Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            UserId INT NOT NULL,
            AccessLevel NVARCHAR(50) NULL DEFAULT 'Full',
            Department NVARCHAR(100) NULL,
            CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
            FOREIGN KEY (UserId) REFERENCES [dbo].[Users](Id)
        )
    """)
    conn.commit()
    print("SUCCESS: SystemAdmins table created")
    
    # Create BackOffice table
    print("Creating BackOffice table...")
    cursor.execute("""
        CREATE TABLE [dbo].[BackOffice] (
            Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            UserId INT NOT NULL,
            Department NVARCHAR(100) NULL,
            ShiftTiming NVARCHAR(50) NULL,
            CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
            FOREIGN KEY (UserId) REFERENCES [dbo].[Users](Id)
        )
    """)
    conn.commit()
    print("SUCCESS: BackOffice table created")
    
    print("\n" + "=" * 60)
    print("All tables created successfully!")
    
    cursor.close()
    conn.close()
    
except Exception as e:
    print(f"Error: {e}")
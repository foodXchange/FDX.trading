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
    
    tables_to_create = [
        ("Suppliers", """
            CREATE TABLE [dbo].[Suppliers] (
                Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                UserId INT NOT NULL,
                CompanyId INT NULL,
                SupplierType NVARCHAR(50) NULL,
                CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
                FOREIGN KEY (UserId) REFERENCES [dbo].[Users](Id),
                FOREIGN KEY (CompanyId) REFERENCES [dbo].[Companies](Id)
            )
        """),
        ("Experts", """
            CREATE TABLE [dbo].[Experts] (
                Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                UserId INT NOT NULL,
                Specialization NVARCHAR(200) NULL,
                CertificationNumber NVARCHAR(50) NULL,
                CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
                FOREIGN KEY (UserId) REFERENCES [dbo].[Users](Id)
            )
        """),
        ("Agents", """
            CREATE TABLE [dbo].[Agents] (
                Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                UserId INT NOT NULL,
                AgencyName NVARCHAR(200) NULL,
                TerritoryRegion NVARCHAR(100) NULL,
                CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
                FOREIGN KEY (UserId) REFERENCES [dbo].[Users](Id)
            )
        """),
        ("SystemAdmins", """
            CREATE TABLE [dbo].[SystemAdmins] (
                Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                UserId INT NOT NULL,
                AccessLevel NVARCHAR(50) NULL DEFAULT 'Full',
                Department NVARCHAR(100) NULL,
                CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
                FOREIGN KEY (UserId) REFERENCES [dbo].[Users](Id)
            )
        """),
        ("BackOffice", """
            CREATE TABLE [dbo].[BackOffice] (
                Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                UserId INT NOT NULL,
                Department NVARCHAR(100) NULL,
                ShiftTiming NVARCHAR(50) NULL,
                CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
                FOREIGN KEY (UserId) REFERENCES [dbo].[Users](Id)
            )
        """)
    ]
    
    for table_name, create_sql in tables_to_create:
        try:
            print(f"Creating {table_name} table...")
            cursor.execute(create_sql)
            conn.commit()
            print(f"SUCCESS: {table_name} table created")
        except Exception as e:
            if "already an object named" in str(e):
                print(f"SKIP: {table_name} table already exists")
            else:
                print(f"ERROR creating {table_name}: {e}")
    
    print("\n" + "=" * 60)
    print("Table creation completed!")
    
    cursor.close()
    conn.close()
    
except Exception as e:
    print(f"Connection error: {e}")
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
    
    # Insert test users for each role
    test_users = [
        # Buyers
        ('Mr.', 'John', 'Buyer1', 'buyer1@test.com', 'Buyer'),
        ('Ms.', 'Jane', 'Buyer2', 'buyer2@test.com', 'Buyer'),
        ('Mr.', 'Bob', 'Buyer3', 'buyer3@test.com', 'Buyer'),
        ('Ms.', 'Alice', 'Buyer4', 'buyer4@test.com', 'Buyer'),
        ('Mr.', 'Tom', 'Buyer5', 'buyer5@test.com', 'Buyer'),
        
        # Suppliers
        ('Mr.', 'Sam', 'Supplier1', 'supplier1@test.com', 'Seller'),
        ('Ms.', 'Sara', 'Supplier2', 'supplier2@test.com', 'Seller'),
        ('Mr.', 'Mike', 'Supplier3', 'supplier3@test.com', 'Seller'),
        ('Ms.', 'Emma', 'Supplier4', 'supplier4@test.com', 'Seller'),
        ('Mr.', 'David', 'Supplier5', 'supplier5@test.com', 'Seller'),
        
        # Experts
        ('Dr.', 'Expert', 'One', 'expert1@test.com', 'Agent'),
        ('Dr.', 'Expert', 'Two', 'expert2@test.com', 'Agent'),
        ('Dr.', 'Expert', 'Three', 'expert3@test.com', 'Agent'),
        ('Dr.', 'Expert', 'Four', 'expert4@test.com', 'Agent'),
        ('Dr.', 'Expert', 'Five', 'expert5@test.com', 'Agent'),
        
        # Agents
        ('Mr.', 'Agent', 'One', 'agent1@test.com', 'Agent'),
        ('Ms.', 'Agent', 'Two', 'agent2@test.com', 'Agent'),
        ('Mr.', 'Agent', 'Three', 'agent3@test.com', 'Agent'),
        ('Ms.', 'Agent', 'Four', 'agent4@test.com', 'Agent'),
        ('Mr.', 'Agent', 'Five', 'agent5@test.com', 'Agent'),
        
        # SystemAdmins
        ('Mr.', 'Admin', 'One', 'admin1@test.com', 'Admin'),
        ('Ms.', 'Admin', 'Two', 'admin2@test.com', 'Admin'),
        ('Mr.', 'Admin', 'Three', 'admin3@test.com', 'Admin'),
        ('Ms.', 'Admin', 'Four', 'admin4@test.com', 'Admin'),
        ('Mr.', 'Admin', 'Five', 'admin5@test.com', 'Admin'),
        
        # BackOffice
        ('Mr.', 'BackOffice', 'One', 'backoffice1@test.com', 'Agent'),
        ('Ms.', 'BackOffice', 'Two', 'backoffice2@test.com', 'Agent'),
        ('Mr.', 'BackOffice', 'Three', 'backoffice3@test.com', 'Agent'),
        ('Ms.', 'BackOffice', 'Four', 'backoffice4@test.com', 'Agent'),
        ('Mr.', 'BackOffice', 'Five', 'backoffice5@test.com', 'Agent')
    ]
    
    print("Inserting test users...")
    inserted_count = 0
    
    for title, first_name, last_name, email, role in test_users:
        try:
            # Check if user already exists
            cursor.execute("SELECT COUNT(*) FROM [dbo].[Users] WHERE Email = ?", email)
            if cursor.fetchone()[0] == 0:
                # Insert user
                cursor.execute("""
                    INSERT INTO [dbo].[Users] (Title, FirstName, LastName, Email, Role, IsActive, CreatedAt, UpdatedAt)
                    VALUES (?, ?, ?, ?, ?, 1, GETUTCDATE(), GETUTCDATE())
                """, title, first_name, last_name, email, role)
                conn.commit()
                inserted_count += 1
                print(f"  Inserted: {email}")
            else:
                print(f"  Skipped (exists): {email}")
        except Exception as e:
            print(f"  Error inserting {email}: {e}")
    
    print(f"\nInserted {inserted_count} new users")
    
    # Now populate the role-specific tables
    print("\n" + "=" * 60)
    print("Populating role-specific tables...")
    
    # Populate Buyers table
    cursor.execute("""
        INSERT INTO [dbo].[Buyers] (UserId, CompanyId, Department)
        SELECT u.Id, 1, 'Procurement' 
        FROM [dbo].[Users] u
        LEFT JOIN [dbo].[Buyers] b ON u.Id = b.UserId
        WHERE u.Email LIKE 'buyer%@test.com' AND b.Id IS NULL
    """)
    conn.commit()
    print("  Populated Buyers table")
    
    # Populate Suppliers table
    cursor.execute("""
        INSERT INTO [dbo].[Suppliers] (UserId, CompanyId, SupplierType)
        SELECT u.Id, 1, 'Food Products' 
        FROM [dbo].[Users] u
        LEFT JOIN [dbo].[Suppliers] s ON u.Id = s.UserId
        WHERE u.Email LIKE 'supplier%@test.com' AND s.Id IS NULL
    """)
    conn.commit()
    print("  Populated Suppliers table")
    
    # Populate Experts table
    cursor.execute("""
        INSERT INTO [dbo].[Experts] (UserId, Specialization, CertificationNumber)
        SELECT u.Id, 'Food Quality', 'CERT-' + CAST(ROW_NUMBER() OVER (ORDER BY u.Id) AS VARCHAR(10))
        FROM [dbo].[Users] u
        LEFT JOIN [dbo].[Experts] e ON u.Id = e.UserId
        WHERE u.Email LIKE 'expert%@test.com' AND e.Id IS NULL
    """)
    conn.commit()
    print("  Populated Experts table")
    
    # Populate Agents table
    cursor.execute("""
        INSERT INTO [dbo].[Agents] (UserId, AgencyName, TerritoryRegion)
        SELECT u.Id, 'FoodX Agency', 'Middle East' 
        FROM [dbo].[Users] u
        LEFT JOIN [dbo].[Agents] a ON u.Id = a.UserId
        WHERE u.Email LIKE 'agent%@test.com' AND a.Id IS NULL
    """)
    conn.commit()
    print("  Populated Agents table")
    
    # Populate SystemAdmins table
    cursor.execute("""
        INSERT INTO [dbo].[SystemAdmins] (UserId, AccessLevel, Department)
        SELECT u.Id, 'Full', 'IT' 
        FROM [dbo].[Users] u
        LEFT JOIN [dbo].[SystemAdmins] sa ON u.Id = sa.UserId
        WHERE u.Email LIKE 'admin%@test.com' AND sa.Id IS NULL
    """)
    conn.commit()
    print("  Populated SystemAdmins table")
    
    # Populate BackOffice table
    cursor.execute("""
        INSERT INTO [dbo].[BackOffice] (UserId, Department, ShiftTiming)
        SELECT u.Id, 'Operations', 'Day Shift' 
        FROM [dbo].[Users] u
        LEFT JOIN [dbo].[BackOffice] bo ON u.Id = bo.UserId
        WHERE u.Email LIKE 'backoffice%@test.com' AND bo.Id IS NULL
    """)
    conn.commit()
    print("  Populated BackOffice table")
    
    print("\n" + "=" * 60)
    print("Test data insertion completed!")
    
    cursor.close()
    conn.close()
    
except Exception as e:
    print(f"Error: {e}")
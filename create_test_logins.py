import pyodbc
import struct
import subprocess
import hashlib
import base64
import secrets

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

# ASP.NET Core Identity password hash function (simplified version)
def hash_password(password):
    """
    Generate ASP.NET Core Identity V3 password hash
    Format: Base64(0x01 + salt[16] + hash[32])
    """
    # Generate 16-byte salt
    salt = secrets.token_bytes(16)
    
    # Use PBKDF2 with SHA256, 100000 iterations (ASP.NET Core Identity V3 defaults)
    iterations = 100000
    dk = hashlib.pbkdf2_hmac('sha256', password.encode('utf-8'), salt, iterations, dklen=32)
    
    # Format: version (0x01) + salt + hash
    result = bytes([0x01]) + salt + dk
    
    # Convert to base64
    return base64.b64encode(result).decode('ascii')

try:
    # Connect using access token
    conn = pyodbc.connect(connection_string, attrs_before={1256: tokenstruct})
    cursor = conn.cursor()
    
    print("Connected to Azure SQL Database")
    print("=" * 80)
    print("CREATING TEST LOGIN CREDENTIALS")
    print("=" * 80)
    
    # Test user credentials (email: password format)
    test_credentials = [
        # Buyers
        ('buyer1@test.com', 'Buyer1@Pass123', 'Buyer'),
        ('buyer2@test.com', 'Buyer2@Pass123', 'Buyer'),
        ('buyer3@test.com', 'Buyer3@Pass123', 'Buyer'),
        ('buyer4@test.com', 'Buyer4@Pass123', 'Buyer'),
        ('buyer5@test.com', 'Buyer5@Pass123', 'Buyer'),
        
        # Suppliers
        ('supplier1@test.com', 'Supplier1@Pass123', 'Seller'),
        ('supplier2@test.com', 'Supplier2@Pass123', 'Seller'),
        ('supplier3@test.com', 'Supplier3@Pass123', 'Seller'),
        ('supplier4@test.com', 'Supplier4@Pass123', 'Seller'),
        ('supplier5@test.com', 'Supplier5@Pass123', 'Seller'),
        
        # Experts
        ('expert1@test.com', 'Expert1@Pass123', 'Expert'),
        ('expert2@test.com', 'Expert2@Pass123', 'Expert'),
        ('expert3@test.com', 'Expert3@Pass123', 'Expert'),
        ('expert4@test.com', 'Expert4@Pass123', 'Expert'),
        ('expert5@test.com', 'Expert5@Pass123', 'Expert'),
        
        # Agents
        ('agent1@test.com', 'Agent1@Pass123', 'Agent'),
        ('agent2@test.com', 'Agent2@Pass123', 'Agent'),
        ('agent3@test.com', 'Agent3@Pass123', 'Agent'),
        ('agent4@test.com', 'Agent4@Pass123', 'Agent'),
        ('agent5@test.com', 'Agent5@Pass123', 'Agent'),
        
        # SystemAdmins
        ('admin1@test.com', 'Admin1@Pass123', 'Admin'),
        ('admin2@test.com', 'Admin2@Pass123', 'Admin'),
        ('admin3@test.com', 'Admin3@Pass123', 'Admin'),
        ('admin4@test.com', 'Admin4@Pass123', 'Admin'),
        ('admin5@test.com', 'Admin5@Pass123', 'Admin'),
        
        # BackOffice
        ('backoffice1@test.com', 'BackOffice1@Pass123', 'BackOffice'),
        ('backoffice2@test.com', 'BackOffice2@Pass123', 'BackOffice'),
        ('backoffice3@test.com', 'BackOffice3@Pass123', 'BackOffice'),
        ('backoffice4@test.com', 'BackOffice4@Pass123', 'BackOffice'),
        ('backoffice5@test.com', 'BackOffice5@Pass123', 'BackOffice')
    ]
    
    print("\nCreating ASP.NET Identity users...")
    print("-" * 40)
    
    created_count = 0
    
    for email, password, role in test_credentials:
        try:
            # Check if user already exists in AspNetUsers
            cursor.execute("SELECT COUNT(*) FROM [dbo].[AspNetUsers] WHERE Email = ?", email)
            if cursor.fetchone()[0] == 0:
                # Generate normalized values
                normalized_email = email.upper()
                normalized_username = email.upper()
                
                # Generate password hash
                password_hash = hash_password(password)
                
                # Generate security stamp (random GUID-like string)
                security_stamp = secrets.token_hex(16).upper()
                
                # Insert into AspNetUsers
                cursor.execute("""
                    INSERT INTO [dbo].[AspNetUsers] 
                    (Id, UserName, NormalizedUserName, Email, NormalizedEmail, 
                     EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
                     PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
                    VALUES (NEWID(), ?, ?, ?, ?, 1, ?, ?, NEWID(), 0, 0, 1, 0)
                """, email, normalized_username, email, normalized_email, password_hash, security_stamp)
                
                conn.commit()
                created_count += 1
                print(f"  Created: {email}")
                
                # Get the user ID we just created
                cursor.execute("SELECT Id FROM [dbo].[AspNetUsers] WHERE Email = ?", email)
                user_id = cursor.fetchone()[0]
                
                # Check if role exists in AspNetRoles
                cursor.execute("SELECT Id FROM [dbo].[AspNetRoles] WHERE Name = ?", role)
                role_result = cursor.fetchone()
                
                if not role_result:
                    # Create role if it doesn't exist
                    role_id = secrets.token_hex(16)
                    cursor.execute("""
                        INSERT INTO [dbo].[AspNetRoles] (Id, Name, NormalizedName, ConcurrencyStamp)
                        VALUES (?, ?, ?, NEWID())
                    """, role_id, role, role.upper())
                    conn.commit()
                    print(f"    Created role: {role}")
                else:
                    role_id = role_result[0]
                
                # Assign user to role
                cursor.execute("""
                    INSERT INTO [dbo].[AspNetUserRoles] (UserId, RoleId)
                    SELECT ?, ?
                    WHERE NOT EXISTS (
                        SELECT 1 FROM [dbo].[AspNetUserRoles] 
                        WHERE UserId = ? AND RoleId = ?
                    )
                """, user_id, role_id, user_id, role_id)
                conn.commit()
                
            else:
                print(f"  Skipped (exists): {email}")
                
        except Exception as e:
            print(f"  Error creating {email}: {e}")
    
    print(f"\nCreated {created_count} new Identity users")
    
    print("\n" + "=" * 80)
    print("TEST LOGIN CREDENTIALS")
    print("=" * 80)
    print("\nYou can now login to the website with these credentials:\n")
    
    print("BUYERS:")
    print("-" * 40)
    for i in range(1, 6):
        print(f"  Email: buyer{i}@test.com")
        print(f"  Password: Buyer{i}@Pass123")
        print()
    
    print("SUPPLIERS:")
    print("-" * 40)
    for i in range(1, 6):
        print(f"  Email: supplier{i}@test.com")
        print(f"  Password: Supplier{i}@Pass123")
        print()
    
    print("EXPERTS:")
    print("-" * 40)
    for i in range(1, 6):
        print(f"  Email: expert{i}@test.com")
        print(f"  Password: Expert{i}@Pass123")
        print()
    
    print("AGENTS:")
    print("-" * 40)
    for i in range(1, 6):
        print(f"  Email: agent{i}@test.com")
        print(f"  Password: Agent{i}@Pass123")
        print()
    
    print("SYSTEM ADMINS:")
    print("-" * 40)
    for i in range(1, 6):
        print(f"  Email: admin{i}@test.com")
        print(f"  Password: Admin{i}@Pass123")
        print()
    
    print("BACK OFFICE:")
    print("-" * 40)
    for i in range(1, 6):
        print(f"  Email: backoffice{i}@test.com")
        print(f"  Password: BackOffice{i}@Pass123")
        print()
    
    print("=" * 80)
    print("All passwords follow the pattern: RoleName#@Pass123")
    print("where # is the user number (1-5)")
    print("=" * 80)
    
    cursor.close()
    conn.close()
    
except Exception as e:
    print(f"Error: {e}")
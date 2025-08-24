import pyodbc
import os
from tabulate import tabulate

# Get connection string from environment
connection_string = os.environ.get('AZURE_SQL_CONNECTION_STRING')

if not connection_string:
    # Fallback to constructed connection string
    connection_string = (
        "Driver={ODBC Driver 17 for SQL Server};"
        "Server=tcp:fdx-sql-prod.database.windows.net,1433;"
        "Database=fdxdb;"
        "Uid=fdxadmin;"
        "Pwd=FDX@2024#SecureDB;"
        "Encrypt=yes;"
        "TrustServerCertificate=no;"
        "Connection Timeout=30;"
    )

try:
    # Connect to database
    conn = pyodbc.connect(connection_string)
    cursor = conn.cursor()
    
    print("=" * 80)
    print("DATABASE CONTENT REPORT")
    print("=" * 80)
    
    # Get table counts
    print("\n📊 TABLE RECORD COUNTS:")
    print("-" * 40)
    
    tables = [
        'AspNetUsers',
        'AspNetRoles', 
        'AspNetUserRoles',
        '__EFMigrationsHistory'
    ]
    
    counts = []
    for table in tables:
        try:
            cursor.execute(f"SELECT COUNT(*) FROM {table}")
            count = cursor.fetchone()[0]
            counts.append([table, count])
        except:
            counts.append([table, "Not found"])
    
    print(tabulate(counts, headers=['Table', 'Record Count'], tablefmt='grid'))
    
    # Show users
    print("\n👥 USERS IN SYSTEM:")
    print("-" * 40)
    
    cursor.execute("""
        SELECT 
            Email,
            UserName,
            FirstName,
            LastName,
            Country,
            EmailConfirmed
        FROM AspNetUsers
        ORDER BY Email
    """)
    
    users = cursor.fetchall()
    if users:
        headers = ['Email', 'UserName', 'FirstName', 'LastName', 'Country', 'Confirmed']
        print(tabulate(users, headers=headers, tablefmt='grid'))
    else:
        print("No users found")
    
    # Show roles
    print("\n🎭 ROLES:")
    print("-" * 40)
    
    cursor.execute("SELECT Id, Name, NormalizedName FROM AspNetRoles ORDER BY Name")
    roles = cursor.fetchall()
    if roles:
        headers = ['Id', 'Name', 'NormalizedName']
        print(tabulate(roles, headers=headers, tablefmt='grid'))
    else:
        print("No roles found")
    
    # Show user-role assignments
    print("\n🔐 USER-ROLE ASSIGNMENTS:")
    print("-" * 40)
    
    cursor.execute("""
        SELECT 
            u.Email,
            CONCAT(u.FirstName, ' ', u.LastName) as FullName,
            r.Name as RoleName
        FROM AspNetUserRoles ur
        INNER JOIN AspNetUsers u ON ur.UserId = u.Id
        INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
        ORDER BY r.Name, u.Email
    """)
    
    assignments = cursor.fetchall()
    if assignments:
        headers = ['Email', 'Full Name', 'Role']
        print(tabulate(assignments, headers=headers, tablefmt='grid'))
    else:
        print("No user-role assignments found")
    
    # Check for other important tables
    print("\n📦 OTHER TABLES IN DATABASE:")
    print("-" * 40)
    
    cursor.execute("""
        SELECT TABLE_NAME
        FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_TYPE = 'BASE TABLE'
        AND TABLE_NAME NOT IN ('AspNetUsers', 'AspNetRoles', 'AspNetUserRoles', '__EFMigrationsHistory')
        ORDER BY TABLE_NAME
    """)
    
    other_tables = cursor.fetchall()
    if other_tables:
        for table in other_tables:
            table_name = table[0]
            cursor.execute(f"SELECT COUNT(*) FROM [{table_name}]")
            count = cursor.fetchone()[0]
            print(f"  • {table_name}: {count} records")
    else:
        print("No other tables found")
    
    print("\n" + "=" * 80)
    print("✅ Database check complete")
    print("=" * 80)
    
    cursor.close()
    conn.close()
    
except Exception as e:
    print(f"❌ Error: {e}")
    import traceback
    traceback.print_exc()
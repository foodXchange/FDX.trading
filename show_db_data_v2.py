import pyodbc
import os

# Use the same connection method as execute_sql.py
def get_connection():
    # Try environment variable first
    conn_str = os.environ.get('AZURE_SQL_CONNECTION_STRING')
    
    if not conn_str:
        # Use Windows Authentication
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

try:
    # Connect to database
    conn = get_connection()
    cursor = conn.cursor()
    
    print("=" * 80)
    print("DATABASE CONTENT REPORT")
    print("=" * 80)
    
    # Get table counts
    print("\nTABLE RECORD COUNTS:")
    print("-" * 40)
    
    tables = [
        'AspNetUsers',
        'AspNetRoles', 
        'AspNetUserRoles',
        '__EFMigrationsHistory'
    ]
    
    for table in tables:
        try:
            cursor.execute(f"SELECT COUNT(*) FROM {table}")
            count = cursor.fetchone()[0]
            print(f"  {table:30} : {count:5} records")
        except:
            print(f"  {table:30} : Not found")
    
    # Show users
    print("\nUSERS IN SYSTEM:")
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
        print(f"\nFound {len(users)} user(s):")
        for user in users:
            print(f"  Email: {user[0]}")
            print(f"    UserName: {user[1]}")
            print(f"    Name: {user[2]} {user[3]}")
            print(f"    Country: {user[4]}")
            print(f"    Email Confirmed: {user[5]}")
            print()
    else:
        print("  No users found")
    
    # Show roles
    print("\nROLES:")
    print("-" * 40)
    
    cursor.execute("SELECT Id, Name, NormalizedName FROM AspNetRoles ORDER BY Name")
    roles = cursor.fetchall()
    if roles:
        for role in roles:
            print(f"  {role[1]:20} (ID: {role[0][:8]}...)")
    else:
        print("  No roles found")
    
    # Show user-role assignments
    print("\nUSER-ROLE ASSIGNMENTS:")
    print("-" * 40)
    
    cursor.execute("""
        SELECT 
            u.Email,
            u.FirstName + ' ' + u.LastName as FullName,
            r.Name as RoleName
        FROM AspNetUserRoles ur
        INNER JOIN AspNetUsers u ON ur.UserId = u.Id
        INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
        ORDER BY r.Name, u.Email
    """)
    
    assignments = cursor.fetchall()
    if assignments:
        for assignment in assignments:
            print(f"  {assignment[0]:30} -> {assignment[2]:20} ({assignment[1]})")
    else:
        print("  No user-role assignments found")
    
    # Check for other important tables
    print("\nOTHER TABLES IN DATABASE:")
    print("-" * 40)
    
    cursor.execute("""
        SELECT TABLE_NAME
        FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_TYPE = 'BASE TABLE'
        AND TABLE_NAME NOT IN ('AspNetUsers', 'AspNetRoles', 'AspNetUserRoles', 
                               'AspNetUserClaims', 'AspNetUserLogins', 'AspNetUserTokens',
                               'AspNetRoleClaims', '__EFMigrationsHistory')
        ORDER BY TABLE_NAME
    """)
    
    other_tables = cursor.fetchall()
    if other_tables:
        for table in other_tables:
            table_name = table[0]
            try:
                cursor.execute(f"SELECT COUNT(*) FROM [{table_name}]")
                count = cursor.fetchone()[0]
                print(f"  {table_name:30} : {count:5} records")
            except:
                print(f"  {table_name:30} : Error accessing")
    else:
        print("  No other application tables found")
    
    print("\n" + "=" * 80)
    print("Database check complete")
    print("=" * 80)
    
    cursor.close()
    conn.close()
    
except Exception as e:
    print(f"Error: {e}")
    import traceback
    traceback.print_exc()
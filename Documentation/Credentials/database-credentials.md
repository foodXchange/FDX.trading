# Database Credentials and Connection Information

## Azure SQL Server Details
- **Server Name**: fdx-sql-prod.database.windows.net
- **Server FQDN**: fdx-sql-prod.database.windows.net
- **Port**: 1433
- **Database Name**: fdxdb
- **Encryption**: Required (TLS/SSL)

## Authentication Methods

### 1. SQL Authentication (Working - Updated)
- **Username**: foodxapp
- **Password**: FoodX@2024!Secure#Trading
- **Status**: ✅ Currently working and tested with FoodX.Simple application

### 2. SQL Authentication (Legacy - Not Working)
- **Username**: fdxadmin
- **Password**: FDX2030! (Note: May need to be reset in Azure Portal)
- **Status**: ❌ Currently not working - needs password reset

### 2. Microsoft Entra Authentication (Recommended)
- **Admin**: Udi Stryk (foodz-x@hotmail.com)
- **Object ID**: 57b7b3d6-90d3-41de-90ba-a4667b260695
- **Authentication**: Microsoft Entra MFA

## Connection Strings

### For Application (appsettings.json) - Working Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:fdx-sql-prod.database.windows.net,1433;Database=fdxdb;User Id=foodxapp;Password=FoodX@2024!Secure#Trading;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### Legacy Connection String (Not Working)
```json
{
  "ConnectionStrings": {
    "FdxDb": "Server=tcp:fdx-sql-prod.database.windows.net,1433;Database=fdxdb;User ID=fdxadmin;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### For Azure AD Authentication
```
Server=tcp:fdx-sql-prod.database.windows.net,1433;
Database=fdxdb;
Authentication=Active Directory Interactive;
Encrypt=True;
TrustServerCertificate=False;
```

## CLI Connection Commands

### Using SQL Authentication (Working - foodxapp)
```bash
sqlcmd -S tcp:fdx-sql-prod.database.windows.net,1433 \
       -d fdxdb \
       -U foodxapp \
       -P "FoodX@2024!Secure#Trading" \
       -C
```

### Using SQL Authentication (Legacy - fdxadmin - needs password reset)
```bash
sqlcmd -S tcp:fdx-sql-prod.database.windows.net,1433 \
       -d fdxdb \
       -U fdxadmin \
       -P "YOUR_PASSWORD" \
       -C
```

### Using Azure AD Authentication
```bash
# Interactive login
sqlcmd -S tcp:fdx-sql-prod.database.windows.net,1433 \
       -d fdxdb \
       -G
```

## Python Connection (pyodbc)

### SQL Authentication (Working - foodxapp)
```python
import pyodbc

server = 'fdx-sql-prod.database.windows.net'
database = 'fdxdb'
username = 'foodxapp'
password = 'FoodX@2024!Secure#Trading'
driver = '{ODBC Driver 17 for SQL Server}'

connection_string = f'DRIVER={driver};SERVER={server};DATABASE={database};UID={username};PWD={password};TrustServerCertificate=yes'
conn = pyodbc.connect(connection_string)
```

### SQL Authentication (Legacy - fdxadmin - needs password reset)
```python
import pyodbc

server = 'fdx-sql-prod.database.windows.net'
database = 'fdxdb'
username = 'fdxadmin'
password = 'YOUR_PASSWORD'  # Needs to be reset in Azure Portal
driver = '{ODBC Driver 17 for SQL Server}'

connection_string = f'DRIVER={driver};SERVER={server};DATABASE={database};UID={username};PWD={password};TrustServerCertificate=yes'
conn = pyodbc.connect(connection_string)
```

### Azure AD Token Authentication
```python
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
tokenstruct = struct.pack('=i', len(exptoken)) + exptoken

# Connect
server = 'fdx-sql-prod.database.windows.net'
database = 'fdxdb'
driver = '{ODBC Driver 17 for SQL Server}'
connection_string = f'DRIVER={driver};SERVER={server};DATABASE={database}'
conn = pyodbc.connect(connection_string, attrs_before={1256: tokenstruct})
```

## SSMS Connection
1. **Server name**: fdx-sql-prod.database.windows.net
2. **Authentication**: Microsoft Entra MFA
3. **User name**: foodz-x@hotmail.com
4. **Database name**: fdxdb
5. **Encrypt**: Mandatory
6. **Trust Server Certificate**: Yes

## Notes
- The database was previously referenced as `fdxdb_v2` in appsettings.json but the actual database name is `fdxdb`
- SQL Authentication with **foodxapp** user is confirmed working (last verified: December 2024)
- SQL Authentication with fdxadmin is currently failing - password needs to be reset in Azure Portal
- Microsoft Entra authentication is working and recommended for production use
- The **foodxapp** credentials are successfully being used by the FoodX.Simple application
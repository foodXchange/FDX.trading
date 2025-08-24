# Database Connection Guide - FoodX Trading Platform

## Quick Start - No Authentication Prompts

We've set up a centralized connection module that uses Azure AD token authentication to minimize authentication prompts.

### Using the Connection Module

1. **Import the module in your Python scripts:**
```python
from db_connection import get_connection

# Get a database connection
conn = get_connection()
cursor = conn.cursor()
cursor.execute("SELECT * FROM FoodXSuppliers")
```

2. **Run SQL files easily:**
```bash
python run_sql.py your_script.sql
```

3. **Execute direct SQL statements:**
```bash
python run_sql.py "SELECT COUNT(*) FROM FoodXBuyers"
```

## Available Database Tools

### Core Modules

1. **db_connection.py**
   - Centralized database connection using Azure AD tokens
   - Automatically handles authentication
   - Falls back to interactive auth if needed

2. **run_sql.py**
   - Simple SQL script runner
   - Supports both files and direct statements
   - Shows query results automatically

3. **execute_sql.py**
   - Original script with Azure AD authentication
   - Used for complex SQL operations

### How It Works

The connection module uses Azure CLI to get an access token:
1. Checks if you're logged into Azure CLI
2. Gets an access token for the database
3. Uses the token to connect without password prompts
4. Token is valid for ~1 hour, then automatically refreshes

### Database Summary (Current State)

| Table | Record Count |
|-------|-------------|
| FoodXBuyers | 671 |
| FoodXSuppliers | 20,215 |
| Exhibitors | 45 |
| AspNetUsers | 1 |

### Enhanced Data Coverage

- **Suppliers with Product Lists**: 3,407
- **Kosher Certified Suppliers**: 439
- **Suppliers with Certifications**: 2,127
- **Suppliers with Year Founded**: 1,287

## Troubleshooting

### If you get authentication prompts:

1. **Ensure Azure CLI is logged in:**
```bash
az login --username foodz-x@hotmail.com
```

2. **Check token is working:**
```bash
az account get-access-token --resource https://database.windows.net/
```

3. **Test the connection:**
```bash
python db_connection.py
```

### Common Issues

- **Token expired**: Run `az login` again
- **Module not found**: Ensure you're in the FDX.trading directory
- **Connection timeout**: Check network connectivity to Azure

## Connection Details

- **Server**: fdx-sql-prod.database.windows.net
- **Database**: fdxdb
- **Authentication**: Azure AD (foodz-x@hotmail.com)
- **Backup Auth**: SQL Authentication (if configured)

## Example Usage

### Import new data
```python
from db_connection import get_connection
import json

conn = get_connection()
cursor = conn.cursor()

with open('new_data.json') as f:
    data = json.load(f)
    
for record in data:
    cursor.execute("INSERT INTO FoodXSuppliers (...) VALUES (...)", params)
    
conn.commit()
```

### Query data
```python
from db_connection import get_connection

conn = get_connection()
cursor = conn.cursor()

cursor.execute("""
    SELECT SupplierName, ProductsList 
    FROM FoodXSuppliers 
    WHERE KosherCertification = 1
""")

for row in cursor.fetchall():
    print(f"{row[0]}: {row[1]}")
```

## Security Notes

- Access tokens are temporary and expire after ~1 hour
- No passwords are stored in code
- Azure AD authentication is more secure than SQL auth
- Always use parameterized queries to prevent SQL injection

## Last Updated
- Date: August 23, 2025
- Database contains full import of buyers, suppliers, and exhibitors
- Enhanced data has been merged successfully
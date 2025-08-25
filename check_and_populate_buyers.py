import pyodbc
import os
from azure.identity import DefaultAzureCredential
from azure.keyvault.secrets import SecretClient

# Get credentials from Azure Key Vault
credential = DefaultAzureCredential()
vault_url = "https://keyvault-fdx-prod.vault.azure.net/"
client = SecretClient(vault_url=vault_url, credential=credential)

# Get the password from Key Vault
password = client.get_secret("foodxapp-password").value

# Connection string
connection_string = f"Driver={{ODBC Driver 17 for SQL Server}};Server=tcp:fdx-sql-prod.database.windows.net,1433;Database=fdxdb;Uid=foodxapp;Pwd={password};Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;"

try:
    conn = pyodbc.connect(connection_string)
    cursor = conn.cursor()
    
    # Check if FoodXBuyers has data
    cursor.execute("SELECT COUNT(*) FROM FoodXBuyers WHERE Company IS NOT NULL")
    count = cursor.fetchone()[0]
    
    print(f"Found {count} buyers with company names in FoodXBuyers table")
    
    if count == 0:
        print("No buyers found. Adding sample buyers...")
        
        # Add sample buyers
        sample_buyers = [
            ("Walmart", "Retail", "North America", "procurement@walmart.com", "USA"),
            ("Carrefour", "Retail", "Europe", "buying@carrefour.com", "France"),
            ("Tesco", "Retail", "Europe", "procurement@tesco.com", "UK"),
            ("Metro AG", "Wholesale", "Europe", "sourcing@metro.de", "Germany"),
            ("Costco", "Wholesale", "North America", "buyers@costco.com", "USA"),
            ("Amazon Fresh", "E-commerce", "Global", "fresh@amazon.com", "USA"),
            ("Kroger", "Retail", "North America", "procurement@kroger.com", "USA"),
            ("Aldi", "Retail", "Europe", "buying@aldi.com", "Germany"),
            ("Lidl", "Retail", "Europe", "sourcing@lidl.com", "Germany"),
            ("Whole Foods", "Retail", "North America", "buyers@wholefoods.com", "USA")
        ]
        
        for company, buyer_type, region, email, country in sample_buyers:
            cursor.execute("""
                INSERT INTO FoodXBuyers (Company, Type, Region, ProcurementEmail, Country)
                VALUES (?, ?, ?, ?, ?)
            """, company, buyer_type, region, email, country)
        
        conn.commit()
        print(f"Added {len(sample_buyers)} sample buyers")
    else:
        # Show top 10 buyers
        cursor.execute("SELECT TOP 10 Company, Type, Region FROM FoodXBuyers WHERE Company IS NOT NULL ORDER BY Company")
        print("\nTop 10 buyers in database:")
        for row in cursor:
            print(f"  - {row[0]} ({row[1]}, {row[2]})")
    
    cursor.close()
    conn.close()
    
except Exception as e:
    print(f"Error: {e}")
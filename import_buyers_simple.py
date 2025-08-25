import pyodbc
import os
from azure.identity import DefaultAzureCredential
from azure.keyvault.secrets import SecretClient

def get_connection_string():
    """Get connection string from Azure Key Vault or environment."""
    try:
        # Try to get from Key Vault first
        credential = DefaultAzureCredential()
        vault_url = "https://fdx-kv-poland.vault.azure.net/"
        client = SecretClient(vault_url=vault_url, credential=credential)
        
        # Get the connection string from Key Vault
        secret = client.get_secret("DefaultConnection")
        return secret.value
    except Exception as e:
        print(f"Could not get from Key Vault: {e}")
        # Fallback to hardcoded for now
        return "Server=fdx-sql-prod.database.windows.net;Database=fdxdb;UID=foodxapp;PWD=Hm7vN9w#Zt3pQ8xK;Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;"

def import_buyers():
    """Import buyers from FoodXBuyers to Companies table."""
    conn_str = get_connection_string()
    
    try:
        conn = pyodbc.connect(conn_str)
        cursor = conn.cursor()
        
        # First check how many we need to import
        cursor.execute("""
            SELECT COUNT(*) 
            FROM FoodXBuyers 
            WHERE Company IS NOT NULL 
            AND Company != ''
            AND Company NOT IN (SELECT Name FROM Companies)
        """)
        total_to_import = cursor.fetchone()[0]
        print(f"Total buyers to import: {total_to_import}")
        
        # Import in batches of 50
        batch_size = 50
        imported = 0
        
        while imported < total_to_import:
            cursor.execute(f"""
                INSERT INTO Companies (Name, CompanyType, BuyerCategory, Country, Website, MainEmail)
                SELECT TOP {batch_size}
                    Company AS Name,
                    'Buyer' AS CompanyType,
                    Type AS BuyerCategory,
                    Region AS Country,
                    Website,
                    ProcurementEmail AS MainEmail
                FROM FoodXBuyers
                WHERE Company IS NOT NULL 
                AND Company != ''
                AND Company NOT IN (SELECT Name FROM Companies)
                ORDER BY Company
            """)
            
            batch_imported = cursor.rowcount
            imported += batch_imported
            conn.commit()
            print(f"Imported {batch_imported} buyers (Total: {imported}/{total_to_import})")
            
            if batch_imported == 0:
                break
        
        # Final verification
        cursor.execute("SELECT COUNT(*) FROM Companies WHERE CompanyType = 'Buyer'")
        total_buyers = cursor.fetchone()[0]
        print(f"\nImport complete! Total buyer companies in database: {total_buyers}")
        
        # Show breakdown by category
        cursor.execute("""
            SELECT BuyerCategory, COUNT(*) as Count
            FROM Companies 
            WHERE CompanyType = 'Buyer'
            GROUP BY BuyerCategory
            ORDER BY Count DESC
        """)
        
        print("\nBuyer Categories:")
        for row in cursor.fetchall():
            print(f"  {row[0] or 'Unknown'}: {row[1]}")
        
        conn.close()
        
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    import_buyers()
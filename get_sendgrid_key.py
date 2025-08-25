import subprocess
import json
import sys

def get_sendgrid_key_from_vault():
    """Retrieve SendGrid API key from Azure Key Vault"""
    
    print("Retrieving SendGrid API key from Azure Key Vault...")
    print("-" * 60)
    
    try:
        # Check if logged in to Azure
        result = subprocess.run(
            ["az", "account", "show"],
            capture_output=True,
            text=True
        )
        
        if result.returncode != 0:
            print("❌ Not logged in to Azure CLI")
            print("\nPlease login first:")
            print("  az login")
            return None
        
        account = json.loads(result.stdout)
        print(f"✅ Logged in as: {account.get('user', {}).get('name', 'Unknown')}")
        print(f"   Subscription: {account.get('name', 'Unknown')}")
        
        # Get SendGrid API key from Key Vault
        print("\nRetrieving SendGrid API key...")
        result = subprocess.run(
            ["az", "keyvault", "secret", "show", 
             "--vault-name", "fdx-kv-poland",
             "--name", "SendGridApiKey",
             "--query", "value",
             "-o", "tsv"],
            capture_output=True,
            text=True
        )
        
        if result.returncode == 0 and result.stdout.strip():
            api_key = result.stdout.strip()
            print(f"✅ SendGrid API key retrieved successfully!")
            print(f"   Key starts with: {api_key[:10]}...")
            print(f"   Key length: {len(api_key)} characters")
            
            # Show how to use it
            print("\n" + "=" * 60)
            print("HOW TO USE THIS KEY:")
            print("=" * 60)
            
            print("\nOption 1: Set as environment variable (Recommended for testing)")
            print("-" * 40)
            print("Windows CMD:")
            print(f'  set SENDGRID_API_KEY={api_key}')
            print("\nWindows PowerShell:")
            print(f"  $env:SENDGRID_API_KEY='{api_key}'")
            
            print("\nOption 2: Update appsettings.Development.json")
            print("-" * 40)
            print('Update FoodX.Admin/appsettings.Development.json:')
            print('{')
            print('  "SendGrid": {')
            print(f'    "ApiKey": "{api_key}",')
            print('    "FromEmail": "udi@fdx.trading",')
            print('    "FromName": "FoodX Platform"')
            print('  },')
            print(f'  "SendGridApiKey": "{api_key}"')
            print('}')
            
            print("\nOption 3: Use in production (Already configured)")
            print("-" * 40)
            print("The key is already in Azure Key Vault.")
            print("Production deployment will automatically use it.")
            
            return api_key
            
        else:
            print("❌ Failed to retrieve SendGrid API key")
            if "ResourceNotFound" in result.stderr:
                print("   The secret 'SendGridApiKey' was not found in vault 'fdx-kv-poland'")
                print("\n   To add it:")
                print('   az keyvault secret set --vault-name fdx-kv-poland --name SendGridApiKey --value "your-sendgrid-api-key"')
            else:
                print(f"   Error: {result.stderr}")
            return None
            
    except Exception as e:
        print(f"❌ Error: {e}")
        return None

if __name__ == "__main__":
    print("=" * 60)
    print("SendGrid API Key Retrieval Tool")
    print("=" * 60)
    
    api_key = get_sendgrid_key_from_vault()
    
    if api_key:
        print("\n" + "=" * 60)
        print("✅ SUCCESS! You can now:")
        print("   1. Run: python test_sendgrid_direct.py")
        print("   2. Update your configuration files")
        print("   3. Test magic link emails")
        print("=" * 60)
    else:
        print("\n" + "=" * 60)
        print("❌ Could not retrieve API key. Please check the errors above.")
        print("=" * 60)
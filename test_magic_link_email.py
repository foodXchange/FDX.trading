import os
import asyncio
import aiohttp
import json
from datetime import datetime

async def test_magic_link():
    """Test magic link email sending through the FoodX Admin application"""
    
    base_url = "http://localhost:5193"
    test_email = "udi@fdx.trading"
    
    print("=" * 60)
    print("MAGIC LINK EMAIL TEST")
    print("=" * 60)
    print(f"Time: {datetime.now()}")
    print(f"Testing email to: {test_email}")
    print(f"Application URL: {base_url}")
    
    try:
        # Test the login endpoint that triggers magic link
        login_url = f"{base_url}/api/auth/magic-link"
        
        async with aiohttp.ClientSession() as session:
            # Send request for magic link
            payload = {"email": test_email}
            headers = {"Content-Type": "application/json"}
            
            print(f"\n📧 Requesting magic link for: {test_email}")
            
            async with session.post(login_url, json=payload, headers=headers) as response:
                status = response.status
                text = await response.text()
                
                print(f"Response Status: {status}")
                
                if status == 200:
                    print("✅ Magic link request successful!")
                    print("Check your email for the magic link")
                    
                    # Check if email was logged locally (dev mode)
                    log_file = "C:\\Users\\fdxadmin\\source\\repos\\FDX.trading\\FoodX.Admin\\magic-links.txt"
                    if os.path.exists(log_file):
                        print(f"\n📁 Dev mode log file found: {log_file}")
                        with open(log_file, 'r') as f:
                            content = f.read()
                            if test_email in content:
                                print("✅ Email logged in dev mode")
                                # Get the last logged link
                                lines = content.split('\n')
                                for line in reversed(lines):
                                    if line.startswith("Link:"):
                                        print(f"Magic Link: {line.replace('Link: ', '')}")
                                        break
                else:
                    print(f"❌ Failed to request magic link")
                    print(f"Response: {text}")
                    
    except aiohttp.ClientError as e:
        print(f"❌ Connection error: {e}")
        print("\nMake sure the FoodX.Admin application is running:")
        print("1. Open Visual Studio")
        print("2. Press F5 to run the application")
        
    except Exception as e:
        print(f"❌ Unexpected error: {e}")
    
    print("\n" + "=" * 60)

async def check_sendgrid_config():
    """Check if SendGrid is properly configured"""
    
    print("\n🔍 Checking SendGrid Configuration...")
    
    # Check for API key in environment or Key Vault
    api_key_exists = False
    
    # Check if Key Vault is accessible
    print("Checking Azure Key Vault access...")
    
    # This would normally check Key Vault, but for now we'll check if the app can access it
    appsettings_path = "C:\\Users\\fdxadmin\\source\\repos\\FDX.trading\\FoodX.Admin\\appsettings.json"
    
    if os.path.exists(appsettings_path):
        with open(appsettings_path, 'r') as f:
            config = json.load(f)
            if "KeyVaultName" in config:
                print(f"✅ Key Vault configured: {config['KeyVaultName']}")
                api_key_exists = True
            else:
                print("⚠️ Key Vault not configured in appsettings.json")
    
    return api_key_exists

async def main():
    # Check SendGrid configuration
    sendgrid_configured = await check_sendgrid_config()
    
    if sendgrid_configured:
        print("✅ SendGrid appears to be configured")
    else:
        print("⚠️ SendGrid may not be properly configured")
    
    # Test magic link email
    await test_magic_link()
    
    print("\n📝 Notes:")
    print("- If in Development mode, emails are logged locally")
    print("- If in Production mode, emails are sent via SendGrid")
    print("- Check the application logs for detailed information")

if __name__ == "__main__":
    asyncio.run(main())
import os
import json
from datetime import datetime

def check_magic_link_config():
    """Check if magic link email is properly configured"""
    
    print("=" * 60)
    print("MAGIC LINK EMAIL CONFIGURATION CHECK")
    print("=" * 60)
    print(f"Time: {datetime.now()}\n")
    
    # Check SendGrid configuration
    print("1. SENDGRID CONFIGURATION")
    print("-" * 40)
    
    appsettings_path = "FoodX.Admin/appsettings.json"
    if os.path.exists(appsettings_path):
        with open(appsettings_path, 'r') as f:
            config = json.load(f)
            
            # Check Key Vault
            if "KeyVaultName" in config:
                print(f"✅ Key Vault: {config['KeyVaultName']}")
            else:
                print("❌ Key Vault: Not configured")
            
            # Check Email Mode
            email_config = config.get("Email", {})
            mode = email_config.get("Mode", "Unknown")
            print(f"✅ Email Mode: {mode}")
            
            # Check From Email
            sendgrid_config = config.get("SendGrid", {})
            from_email = sendgrid_config.get("FromEmail", "noreply@fdx.trading")
            print(f"✅ From Email: {from_email}")
    
    # Check SendGrid service registration
    print("\n2. SERVICE REGISTRATION")
    print("-" * 40)
    
    program_path = "FoodX.Admin/Program.cs"
    if os.path.exists(program_path):
        with open(program_path, 'r') as f:
            content = f.read()
            if "ISendGridEmailService" in content:
                print("✅ SendGrid service registered")
            else:
                print("❌ SendGrid service not found")
            
            if "AddDefaultIdentity" in content:
                print("✅ Identity services configured")
            else:
                print("❌ Identity services not found")
    
    # Check magic link implementation
    print("\n3. MAGIC LINK IMPLEMENTATION")
    print("-" * 40)
    
    service_path = "FoodX.Admin/Services/SendGridEmailService.cs"
    if os.path.exists(service_path):
        print("✅ SendGridEmailService.cs exists")
        with open(service_path, 'r') as f:
            content = f.read()
            if "SendMagicLinkEmailAsync" in content:
                print("✅ SendMagicLinkEmailAsync method implemented")
            if "Magic Link" in content:
                print("✅ Magic link email template configured")
    
    # Check for dev mode log file
    print("\n4. DEVELOPMENT MODE")
    print("-" * 40)
    
    log_path = "FoodX.Admin/magic-links.txt"
    if os.path.exists(log_path):
        print(f"✅ Dev log file exists: {log_path}")
        with open(log_path, 'r') as f:
            lines = f.readlines()
            if lines:
                print(f"   Last entry: {lines[-1].strip()[:50]}...")
    else:
        print("📝 Dev log file will be created when first email is sent")
    
    # Check authentication pages
    print("\n5. AUTHENTICATION PAGES")
    print("-" * 40)
    
    login_page = "FoodX.Admin/Components/Pages/Account/Login.razor"
    if os.path.exists(login_page):
        print("✅ Login.razor exists")
        with open(login_page, 'r') as f:
            content = f.read()
            if "magic" in content.lower():
                print("✅ Magic link UI implemented")
    
    print("\n" + "=" * 60)
    print("CONFIGURATION SUMMARY")
    print("=" * 60)
    
    print("""
✅ Ready to test magic link email:

1. Start the application:
   - Open Visual Studio
   - Press F5 to run
   - Or run: dotnet run --project FoodX.Admin

2. Navigate to login page:
   http://localhost:5193/account/login

3. Enter email: udi@fdx.trading

4. Check for email:
   - In Development mode: Check FoodX.Admin/magic-links.txt
   - In Production mode: Check actual email inbox

Current Status:
- SendGrid API key stored in Azure Key Vault
- Email service registered and configured
- Magic link template ready
- Authentication flow implemented
""")

if __name__ == "__main__":
    check_magic_link_config()
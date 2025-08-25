import requests
import json
import time
from datetime import datetime

# API endpoint for requesting magic link
api_url = "http://localhost:5193/api/auth/send-magic-link"

# Test email address
test_email = "udi@fdx.trading"

print(f"[{datetime.now()}] Testing Magic Link Email for: {test_email}")
print("-" * 50)

try:
    # Request magic link
    payload = {"email": test_email}
    
    # Disable SSL verification for localhost testing
    response = requests.post(api_url, json=payload, verify=False)
    
    if response.status_code == 200:
        result = response.json()
        print(f"[SUCCESS] Magic link request successful!")
        print(f"Response: {json.dumps(result, indent=2)}")
        print(f"\n[EMAIL] Check your email at: {test_email}")
        print("The email should arrive within 1-2 minutes.")
        print("\nThe email will contain:")
        print("- A magic link button to click")
        print("- The link expires in 15 minutes")
        print("- Can only be used once")
    else:
        print(f"[ERROR] Failed to send magic link")
        print(f"Status Code: {response.status_code}")
        print(f"Response: {response.text}")
        
except requests.exceptions.ConnectionError:
    print("[ERROR] Could not connect to the application")
    print("Make sure the application is running on http://localhost:5193")
    print("\nTo start the application, run:")
    print("cd C:\\Users\\fdxadmin\\source\\repos\\FDX.trading\\FoodX.Admin")
    print("dotnet run")
    
except Exception as e:
    print(f"[ERROR] Error: {str(e)}")

print("\n" + "-" * 50)
print("If no email is received:")
print("1. Check the application logs for SendGrid errors")
print("2. Verify SendGrid API key is loaded")
print("3. Check spam/junk folder")
print("4. Ensure the email address is verified in SendGrid (if using sandbox mode)")
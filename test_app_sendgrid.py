import requests
import json
import sys
import time

BASE_URL = "http://localhost:5193"

def test_magic_link_with_sendgrid():
    """Test magic link email sending through the application"""
    
    print("Testing magic link functionality with SendGrid...")
    print("-" * 50)
    
    # Request a magic link
    email = "udi@fdx.trading"
    
    print(f"Requesting magic link for: {email}")
    
    try:
        # First get the login page to get any required tokens
        session = requests.Session()
        login_page = session.get(f"{BASE_URL}/Account/Login")
        
        # Now request the magic link
        response = session.post(
            f"{BASE_URL}/Account/Login",
            data={"Email": email, "IsMagicLink": "true"},
            headers={"Content-Type": "application/x-www-form-urlencoded"},
            allow_redirects=False
        )
        
        print(f"Response status: {response.status_code}")
        
        if response.status_code in [200, 302]:
            print("SUCCESS: Magic link request processed")
            print("Check your email for the magic link")
            
            # Check if we got a redirect (success case)
            if response.status_code == 302:
                location = response.headers.get('Location', '')
                print(f"Redirect to: {location}")
                if 'MagicLinkSent' in location or 'success' in location.lower():
                    print("Magic link email should have been sent via SendGrid!")
                    return True
            
            # For 200 response, check content
            if 'sent' in response.text.lower() or 'check your email' in response.text.lower():
                print("Magic link email appears to have been sent!")
                return True
                
        else:
            print(f"FAILED: Unexpected status code: {response.status_code}")
            print(f"Response: {response.text[:500]}")
            return False
            
    except Exception as e:
        print(f"ERROR: {str(e)}")
        return False

def check_application_status():
    """Check if the application is running"""
    try:
        response = requests.get(BASE_URL, timeout=5)
        if response.status_code == 200:
            print("Application is running at http://localhost:5193")
            return True
    except:
        pass
    
    print("Application is not running. Please start it with 'dotnet run' in the FoodX.Admin directory")
    return False

if __name__ == "__main__":
    if check_application_status():
        print()
        success = test_magic_link_with_sendgrid()
        sys.exit(0 if success else 1)
    else:
        sys.exit(1)
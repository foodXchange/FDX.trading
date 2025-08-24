import requests
import json
import sys

BASE_URL = "http://localhost:5193"

def test_magic_link_api():
    """Test the magic link API endpoint directly"""
    
    print("Testing Magic Link API endpoint...")
    print("-" * 50)
    
    email = "udi@fdx.trading"
    
    # Call the API endpoint directly
    api_url = f"{BASE_URL}/api/auth/magic-link"
    
    print(f"POST {api_url}")
    print(f"Email: {email}")
    
    try:
        response = requests.post(
            api_url,
            json={"email": email},
            headers={"Content-Type": "application/json"}
        )
        
        print(f"Response status: {response.status_code}")
        
        if response.status_code == 200:
            print("SUCCESS: Magic link request accepted")
            result = response.json()
            print(f"Response: {json.dumps(result, indent=2)}")
            return True
        else:
            print(f"Status code: {response.status_code}")
            print(f"Response: {response.text}")
            return False
            
    except Exception as e:
        print(f"ERROR: {str(e)}")
        return False

if __name__ == "__main__":
    success = test_magic_link_api()
    sys.exit(0 if success else 1)
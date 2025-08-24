import requests
import json

# Test the magic link email endpoint
url = "http://localhost:5193/api/test-magic-link"
headers = {"Content-Type": "application/json"}
data = {"email": "udi@fdx.trading"}

print("Testing magic link HTML email...")
print(f"URL: {url}")
print(f"Email: {data['email']}")

try:
    # First, let's check if the app is running
    base_url = "http://localhost:5193"
    response = requests.get(base_url, timeout=5)
    if response.status_code == 200:
        print(f"✓ Application is running at {base_url}")
    else:
        print(f"⚠ Application returned status {response.status_code}")
        
    # Check the magic link page
    magic_link_url = f"{base_url}/Account/MagicLink"
    response = requests.get(magic_link_url, timeout=5)
    if response.status_code == 200:
        print(f"✓ Magic link page is accessible")
    else:
        print(f"⚠ Magic link page returned status {response.status_code}")
        
except Exception as e:
    print(f"Error: {e}")
    print("\nMake sure the application is running with: dotnet run")
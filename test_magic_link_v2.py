import requests
import time

# Test accessing the magic link page
base_url = "http://localhost:5193"

print("Testing FoodX Magic Link System...")
print("=" * 50)

# Try to access the magic link page
magic_link_url = f"{base_url}/Account/MagicLink"
response = requests.get(magic_link_url)
print(f"GET /Account/MagicLink - Status: {response.status_code}")

# Check if we can access the home page
home_response = requests.get(base_url)
print(f"GET / - Status: {home_response.status_code}")

# Now let's check if a magic links file exists
import os
magic_links_path = r"C:\Users\fdxadmin\source\repos\FDX.trading\FoodX.Admin\magic-links.txt"

print("\n" + "=" * 50)
print("Checking for existing magic links...")

if os.path.exists(magic_links_path):
    print(f"Found magic-links.txt at: {magic_links_path}")
    with open(magic_links_path, 'r') as f:
        content = f.read()
        if content.strip():
            print("\n📧 Email Log Contents:")
            print(content)
        else:
            print("File exists but is empty.")
else:
    print("No magic-links.txt file found yet.")
    print("\nTo generate a magic link:")
    print("1. Open your browser and go to: http://localhost:5193/Account/MagicLink")
    print("2. Enter email: udi@fdx.trading")
    print("3. Click 'Send Login Link'")
    print("\nThe magic link will be saved to magic-links.txt since SendGrid is not configured.")
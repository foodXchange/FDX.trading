import requests
import json

# Test the magic link request endpoint
url = "http://localhost:5193/Account/MagicLinkRequest"
email = "udi@fdx.trading"

# First, make a GET request to get the page
get_response = requests.get(url)
print(f"GET Status: {get_response.status_code}")

# Try to request a magic link via POST
post_data = {
    "Email": email
}

headers = {
    "Content-Type": "application/x-www-form-urlencoded"
}

# Convert data to form-encoded format
form_data = f"Email={email}"

post_response = requests.post(url, data=form_data, headers=headers, allow_redirects=False)
print(f"POST Status: {post_response.status_code}")
print(f"Location: {post_response.headers.get('Location', 'No redirect')}")

# Check if the magic-links.txt file was created
import os
magic_links_path = r"C:\Users\fdxadmin\source\repos\FDX.trading\FoodX.Admin\magic-links.txt"
if os.path.exists(magic_links_path):
    print("\nMagic links file found!")
    with open(magic_links_path, 'r') as f:
        content = f.read()
        print("Content:")
        print(content)
else:
    print("\nMagic links file not found.")
    
# Check the current directory for the file
current_dir_path = "magic-links.txt"
if os.path.exists(current_dir_path):
    print("\nMagic links file found in current directory!")
    with open(current_dir_path, 'r') as f:
        content = f.read()
        print("Content:")
        print(content)
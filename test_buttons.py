import requests
import json
from datetime import datetime

# Base URL
BASE_URL = "http://localhost:5193"

# List of all routes to test
ROUTES = [
    # Main pages
    ("/", "Home/Dashboard"),
    ("/auth", "Auth page"),
    ("/Account/MagicLink", "Magic Link Login"),
    ("/Account/Manage", "Account Management"),
    ("/Account/Logout", "Logout"),
    
    # Buyer Portal Routes
    ("/portal/buyer/dashboard", "Buyer Dashboard"),
    ("/portal/buyer/rfqs", "Buyer RFQs"),
    ("/portal/buyer/quotes", "Buyer Quotes"),
    ("/portal/buyer/orders", "Buyer Orders"),
    ("/portal/buyer/rfq/create", "Create RFQ"),
    ("/portal/buyer/suppliers", "Browse Suppliers"),
    ("/portal/buyer/ai-search", "AI Search"),
    ("/portal/buyer/vendors/approved", "Approved Vendors"),
    
    # Supplier Portal Routes
    ("/portal/supplier/dashboard", "Supplier Dashboard"),
    ("/portal/supplier/products", "Supplier Products"),
    ("/portal/supplier/products/add", "Add Product"),
    ("/portal/supplier/products/import", "Import Products"),
    ("/portal/supplier/rfqs", "Supplier RFQs"),
    ("/portal/supplier/quotes", "Supplier Quotes"),
    ("/portal/supplier/orders", "Supplier Orders"),
    ("/portal/supplier/profile", "Supplier Profile"),
    
    # Admin Routes
    ("/users", "Users Management"),
    ("/companies", "Companies Management"),
    ("/suppliers", "Suppliers List"),
    ("/buyers", "Buyers List"),
    ("/products", "Products List"),
    ("/settings", "Settings"),
    ("/analytics", "Analytics"),
    ("/reports", "Reports"),
    
    # SuperAdmin Routes
    ("/superadmin/dashboard", "SuperAdmin Dashboard"),
    ("/superadmin/impersonate", "User Impersonation"),
    
    # Other Routes
    ("/help", "Help & Support"),
]

def test_route(route, description):
    """Test if a route returns a valid response"""
    url = BASE_URL + route
    try:
        response = requests.get(url, timeout=5, allow_redirects=False)
        status = response.status_code
        
        if status == 200:
            return "[OK]", "OK"
        elif status == 302 or status == 301:
            redirect_to = response.headers.get('Location', 'Unknown')
            return "[REDIRECT]", f"Redirect to {redirect_to}"
        elif status == 401:
            return "[AUTH]", "Unauthorized (Login Required)"
        elif status == 403:
            return "[FORBIDDEN]", "Forbidden"
        elif status == 404:
            return "[404]", "Not Found"
        else:
            return "[WARN]", f"Status {status}"
            
    except requests.exceptions.ConnectionError:
        return "[ERROR]", "Connection Error"
    except requests.exceptions.Timeout:
        return "[TIMEOUT]", "Timeout"
    except Exception as e:
        return "[EXCEPTION]", str(e)

def main():
    print("=" * 80)
    print(f"FoodX.Admin Button Route Testing")
    print(f"Date: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    print(f"Base URL: {BASE_URL}")
    print("=" * 80)
    print()
    
    results = []
    working_count = 0
    redirect_count = 0
    auth_required_count = 0
    not_found_count = 0
    
    for route, description in ROUTES:
        icon, status = test_route(route, description)
        results.append((route, description, icon, status))
        
        if icon == "[OK]":
            working_count += 1
        elif icon == "[REDIRECT]":
            redirect_count += 1
        elif icon == "[AUTH]":
            auth_required_count += 1
        elif icon == "[404]":
            not_found_count += 1
        
        print(f"{icon} {route:<40} {description:<30} [{status}]")
    
    print()
    print("=" * 80)
    print("SUMMARY:")
    print(f"Total Routes Tested: {len(ROUTES)}")
    print(f"[OK] Working: {working_count}")
    print(f"[REDIRECT] Redirects: {redirect_count}")
    print(f"[AUTH] Auth Required: {auth_required_count}")
    print(f"[404] Not Found: {not_found_count}")
    print(f"[OTHER] Other Issues: {len(ROUTES) - working_count - redirect_count - auth_required_count - not_found_count}")
    print("=" * 80)
    
    # Save results to JSON
    with open('button_test_results.json', 'w') as f:
        json.dump({
            'timestamp': datetime.now().isoformat(),
            'base_url': BASE_URL,
            'total_routes': len(ROUTES),
            'results': [
                {
                    'route': r[0],
                    'description': r[1],
                    'status': r[3],
                    'icon': r[2]
                } for r in results
            ],
            'summary': {
                'working': working_count,
                'redirects': redirect_count,
                'auth_required': auth_required_count,
                'not_found': not_found_count
            }
        }, f, indent=2)
    
    print(f"\nResults saved to button_test_results.json")

if __name__ == "__main__":
    main()
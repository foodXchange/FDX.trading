import json

# Check buyers JSON structure
print("Checking buyers JSON structure...")
with open(r"G:\My Drive\Business intelligence\food_buyers_database.json", 'r', encoding='utf-8') as f:
    data = json.load(f)

print(f"Keys in JSON: {list(data.keys())}")

if 'all_buyers' in data:
    buyers = data['all_buyers']
    print(f"Number of buyers: {len(buyers)}")
    
    if buyers and len(buyers) > 0:
        print("\nFirst buyer structure:")
        first = buyers[0]
        for key, value in first.items():
            print(f"  {key}: {type(value).__name__} = {str(value)[:50] if value else 'None'}...")

# Check exhibitors JSON structure
print("\n" + "="*50)
print("Checking exhibitors JSON structure...")
with open(r"G:\My Drive\Business intelligence\accessible_exhibitions_exhibitors.json", 'r', encoding='utf-8') as f:
    exhibitors = json.load(f)

print(f"Type: {type(exhibitors).__name__}")
print(f"Number of exhibitors: {len(exhibitors)}")

if exhibitors and len(exhibitors) > 0:
    print("\nFirst exhibitor structure:")
    first = exhibitors[0]
    for key, value in first.items():
        print(f"  {key}: {type(value).__name__} = {str(value)[:50] if value else 'None'}...")
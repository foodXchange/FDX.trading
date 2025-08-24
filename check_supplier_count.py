import json

print("Checking supplier JSON file...")
file_path = r"G:\My Drive\Business intelligence\food_suppliers_database.json"

print(f"Loading {file_path}...")
with open(file_path, 'r', encoding='utf-8') as f:
    data = json.load(f)

print(f"\nKeys in JSON: {list(data.keys())}")

# Check different possible structures
if 'suppliers' in data:
    suppliers = data['suppliers']
    print(f"Found 'suppliers' key with {len(suppliers)} entries")
    
    if suppliers and len(suppliers) > 0:
        print("\nFirst supplier structure:")
        first = suppliers[0]
        for key in list(first.keys())[:10]:  # Show first 10 keys
            value = first[key]
            if value is None:
                print(f"  {key}: None")
            elif isinstance(value, str):
                print(f"  {key}: '{value[:50]}...'")
            else:
                print(f"  {key}: {type(value).__name__}")

# Check for other possible keys
for key in data.keys():
    if key != 'suppliers' and isinstance(data[key], list):
        print(f"\nFound '{key}' key with {len(data[key])} entries")

# Total count
total_suppliers = 0
for key, value in data.items():
    if isinstance(value, list):
        total_suppliers += len(value)
        
print(f"\nTotal supplier entries across all keys: {total_suppliers}")
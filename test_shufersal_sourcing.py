#!/usr/bin/env python
"""
Test Shufersal use case: Finding kosher candy and snacks suppliers
Shufersal is a major Israeli supermarket chain that requires kosher products
"""
from db_connection import get_connection

def find_kosher_suppliers(product_keywords, limit=20):
    """Find suppliers with kosher products matching keywords"""
    try:
        conn = get_connection()
        cursor = conn.cursor()
        
        # Build search query
        search_conditions = []
        params = []
        
        for keyword in product_keywords:
            search_conditions.append("(ProductName LIKE ? OR Category LIKE ?)")
            params.extend([f"%{keyword}%", f"%{keyword}%"])
        
        where_clause = " OR ".join(search_conditions)
        
        # Find kosher products matching keywords
        query = f"""
            SELECT 
                SupplierName,
                ProductName,
                Category,
                Country,
                IsKosher,
                IsHalal,
                IsOrganic
            FROM SupplierProducts
            WHERE ({where_clause})
            AND (IsKosher = 1 OR Country = 'Israel')
            ORDER BY 
                CASE WHEN Country = 'Israel' THEN 0 ELSE 1 END,
                IsKosher DESC,
                SupplierName
        """
        
        # Limit results
        if limit:
            query = query.replace("SELECT ", f"SELECT TOP {limit} ")
        
        cursor.execute(query, params)
        results = cursor.fetchall()
        
        cursor.close()
        conn.close()
        
        return results
        
    except Exception as e:
        print(f"Error: {e}")
        return []

def test_shufersal_case():
    """Test finding suppliers for Shufersal's kosher candy and snacks request"""
    
    print("=" * 80)
    print("SHUFERSAL USE CASE: Finding Kosher Candy and Snacks Suppliers")
    print("=" * 80)
    
    # Shufersal's requirements
    print("\nShufersal Requirements:")
    print("  - Must be Kosher certified")
    print("  - Product categories: Candy, Snacks, Confectionery")
    print("  - Preference for Israeli suppliers")
    
    # Search for candy suppliers
    print("\n1. SEARCHING FOR KOSHER CANDY SUPPLIERS:")
    print("-" * 40)
    
    candy_results = find_kosher_suppliers(['candy', 'confection', 'chocolate', 'sweet'], limit=10)
    
    if candy_results:
        print(f"Found {len(candy_results)} kosher candy products:\n")
        for supplier, product, category, country, is_kosher, is_halal, is_organic in candy_results:
            certifications = []
            if is_kosher:
                certifications.append("Kosher")
            if is_halal:
                certifications.append("Halal")
            if is_organic:
                certifications.append("Organic")
            cert_str = ", ".join(certifications) if certifications else "None"
            
            print(f"  Supplier: {supplier[:40]:40}")
            print(f"    Product: {product[:50]}")
            print(f"    Country: {country:20} | Category: {category}")
            print(f"    Certifications: {cert_str}")
            print()
    else:
        print("No kosher candy suppliers found")
    
    # Search for snacks suppliers
    print("\n2. SEARCHING FOR KOSHER SNACKS SUPPLIERS:")
    print("-" * 40)
    
    snacks_results = find_kosher_suppliers(['snack', 'chips', 'crackers', 'popcorn'], limit=10)
    
    if snacks_results:
        print(f"Found {len(snacks_results)} kosher snack products:\n")
        for supplier, product, category, country, is_kosher, is_halal, is_organic in snacks_results:
            certifications = []
            if is_kosher:
                certifications.append("Kosher")
            if is_halal:
                certifications.append("Halal")
            if is_organic:
                certifications.append("Organic")
            cert_str = ", ".join(certifications) if certifications else "None"
            
            print(f"  Supplier: {supplier[:40]:40}")
            print(f"    Product: {product[:50]}")
            print(f"    Country: {country:20} | Category: {category}")
            print(f"    Certifications: {cert_str}")
            print()
    else:
        print("No kosher snacks suppliers found")
    
    # Get statistics
    try:
        conn = get_connection()
        cursor = conn.cursor()
        
        # Count Israeli suppliers
        cursor.execute("""
            SELECT COUNT(DISTINCT SupplierName) 
            FROM SupplierProducts 
            WHERE Country = 'Israel'
        """)
        israeli_suppliers = cursor.fetchone()[0]
        
        # Count kosher products
        cursor.execute("""
            SELECT COUNT(*) 
            FROM SupplierProducts 
            WHERE IsKosher = 1
        """)
        kosher_products = cursor.fetchone()[0]
        
        print("\n" + "=" * 80)
        print("SUMMARY STATISTICS:")
        print(f"  Israeli suppliers in database: {israeli_suppliers}")
        print(f"  Total kosher products: {kosher_products}")
        
        cursor.close()
        conn.close()
        
    except Exception as e:
        print(f"Error getting statistics: {e}")
    
    print("\n" + "=" * 80)
    print("NEXT STEPS FOR SHUFERSAL:")
    print("1. Generate RFQ briefs for matched suppliers")
    print("2. Include specific kosher certification requirements")
    print("3. Send personalized emails to each supplier")
    print("4. Track responses and quotes")
    print("=" * 80)

if __name__ == "__main__":
    test_shufersal_case()
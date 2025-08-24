"""
Database connection module for FoodX Trading Platform
Uses Azure AD authentication with access token to avoid repeated prompts
"""
import pyodbc
import subprocess
import struct
import os

def get_access_token():
    """Get Azure AD access token for database authentication"""
    try:
        result = subprocess.run(
            [r'C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd', 
             'account', 'get-access-token', 
             '--resource', 'https://database.windows.net/', 
             '--query', 'accessToken', 
             '-o', 'tsv'], 
            capture_output=True, 
            text=True, 
            shell=True
        )
        token = result.stdout.strip()
        if not token:
            raise Exception("Failed to get access token")
        return token
    except Exception as e:
        print(f"[WARNING] Could not get access token: {e}")
        return None

def get_connection():
    """Get a connection to the Azure SQL Database using SQL authentication"""
    conn_str = (
        'DRIVER={ODBC Driver 17 for SQL Server};'
        'SERVER=fdx-sql-prod.database.windows.net;'
        'DATABASE=fdxdb;'
        'UID=foodxapp;'
        'PWD=FoodX@2024!Secure#Trading;'
        'Encrypt=yes;'
        'TrustServerCertificate=no;'
        'Connection Timeout=30;'
    )
    
    return pyodbc.connect(conn_str)

def get_connection_string():
    """Get the connection string for Entity Framework configuration"""
    return (
        "Server=tcp:fdx-sql-prod.database.windows.net,1433;"
        "Initial Catalog=fdxdb;"
        "Persist Security Info=False;"
        "MultipleActiveResultSets=False;"
        "Encrypt=True;"
        "TrustServerCertificate=False;"
        "Connection Timeout=30;"
        "Authentication=Active Directory Interactive;"
    )

def test_connection():
    """Test the database connection"""
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute("SELECT @@VERSION, SYSTEM_USER")
        row = cursor.fetchone()
        print("[SUCCESS] Connected to database!")
        print(f"Server version: {row[0][:50]}...")
        print(f"Connected as: {row[1]}")
        cursor.close()
        conn.close()
        return True
    except Exception as e:
        print(f"[ERROR] Connection failed: {e}")
        return False

if __name__ == "__main__":
    test_connection()
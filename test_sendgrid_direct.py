import os
import requests
import json

def test_sendgrid_direct():
    """Test SendGrid API directly with the key from Azure Key Vault or environment"""
    
    # Try to get API key from environment or use a test key
    api_key = os.environ.get('SENDGRID_API_KEY', '')
    
    if not api_key:
        print("❌ SENDGRID_API_KEY environment variable not set")
        print("\nTo test SendGrid, you need to:")
        print("1. Get the API key from Azure Key Vault (fdx-kv-poland)")
        print("2. Set it as environment variable:")
        print("   Windows: set SENDGRID_API_KEY=your-key-here")
        print("   PowerShell: $env:SENDGRID_API_KEY='your-key-here'")
        return False
    
    # Test API key validity
    headers = {
        'Authorization': f'Bearer {api_key}',
        'Content-Type': 'application/json'
    }
    
    # Test 1: Verify API key
    print("Testing SendGrid API key...")
    response = requests.get(
        'https://api.sendgrid.com/v3/scopes',
        headers=headers
    )
    
    if response.status_code == 200:
        print("✅ API key is valid!")
        scopes = response.json()
        print(f"   Scopes: {', '.join(scopes.get('scopes', []))[:100]}...")
    else:
        print(f"❌ API key validation failed: {response.status_code}")
        print(f"   Response: {response.text}")
        return False
    
    # Test 2: Send test email
    print("\nSending test email...")
    
    email_data = {
        "personalizations": [{
            "to": [{"email": "udi@fdx.trading"}],
            "subject": "SendGrid Test - FoodX Platform"
        }],
        "from": {
            "email": "no-reply@fdx.trading",
            "name": "FoodX Platform"
        },
        "content": [{
            "type": "text/plain",
            "value": "This is a test email from FoodX Platform to verify SendGrid is working correctly."
        }, {
            "type": "text/html",
            "value": """
            <html>
            <body style="font-family: Arial, sans-serif; padding: 20px;">
                <h2>SendGrid Test Email</h2>
                <p>This is a test email from <strong>FoodX Platform</strong> to verify SendGrid is working correctly.</p>
                <p style="color: green;">✅ If you receive this email, SendGrid is configured properly!</p>
                <hr>
                <p style="font-size: 12px; color: #666;">
                    This is an automated test email. No action required.
                </p>
            </body>
            </html>
            """
        }]
    }
    
    response = requests.post(
        'https://api.sendgrid.com/v3/mail/send',
        headers=headers,
        json=email_data
    )
    
    if response.status_code in [200, 202]:
        print("✅ Test email sent successfully!")
        print("   Check udi@fdx.trading inbox")
        return True
    else:
        print(f"❌ Failed to send email: {response.status_code}")
        print(f"   Response: {response.text}")
        
        # Check if it's a sender verification issue
        if response.status_code == 403:
            print("\n⚠️  Possible issues:")
            print("   1. Sender email (no-reply@fdx.trading) not verified")
            print("   2. Domain authentication not completed")
            print("   3. API key lacks mail send permissions")
        
        return False

if __name__ == "__main__":
    print("=" * 60)
    print("SendGrid Direct API Test")
    print("=" * 60)
    
    success = test_sendgrid_direct()
    
    print("\n" + "=" * 60)
    if success:
        print("✅ SendGrid is working! You can now:")
        print("   1. Update appsettings.Development.json with the API key")
        print("   2. Or keep it in Azure Key Vault for production")
    else:
        print("❌ SendGrid test failed. Please check the issues above.")
    print("=" * 60)
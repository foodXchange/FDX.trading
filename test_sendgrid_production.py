import requests
import json
import sys

# SendGrid API configuration
SENDGRID_API_KEY = "YOUR_SENDGRID_API_KEY_HERE"
SENDGRID_API_URL = "https://api.sendgrid.com/v3/mail/send"

def send_test_email():
    """Send a test email using SendGrid API directly"""
    
    headers = {
        "Authorization": f"Bearer {SENDGRID_API_KEY}",
        "Content-Type": "application/json"
    }
    
    email_data = {
        "personalizations": [{
            "to": [{"email": "udi@fdx.trading"}],
            "subject": "FoodX SendGrid Test - Configuration Verified"
        }],
        "from": {
            "email": "udi@fdx.trading",
            "name": "FoodX Platform"
        },
        "content": [{
            "type": "text/plain",
            "value": "This is a test email to verify SendGrid configuration.\n\nYour SendGrid API key has been successfully configured in Azure Key Vault.\n\nKey Details:\n- API Key: Stored in fdx-kv-poland Key Vault\n- Secret Name: SendGridApiKey\n- From Email: udi@fdx.trading\n- Integration: Ready for production use"
        }, {
            "type": "text/html",
            "value": """
            <html>
            <body style="font-family: Arial, sans-serif; padding: 20px;">
                <h2 style="color: #667eea;">SendGrid Configuration Verified ✅</h2>
                <p>This is a test email confirming that your SendGrid integration is working correctly.</p>
                <div style="background: #f8f9fa; padding: 15px; border-radius: 8px; margin: 20px 0;">
                    <h3>Configuration Details:</h3>
                    <ul>
                        <li><strong>API Key:</strong> Successfully stored in Azure Key Vault</li>
                        <li><strong>Key Vault:</strong> fdx-kv-poland</li>
                        <li><strong>Secret Name:</strong> SendGridApiKey</li>
                        <li><strong>From Email:</strong> udi@fdx.trading</li>
                        <li><strong>Status:</strong> Ready for production use</li>
                    </ul>
                </div>
                <p style="color: #666;">Your FoodX platform can now send emails via SendGrid in production mode.</p>
            </body>
            </html>
            """
        }]
    }
    
    print("Sending test email via SendGrid API...")
    print(f"From: udi@fdx.trading")
    print(f"To: udi@fdx.trading")
    
    try:
        response = requests.post(SENDGRID_API_URL, headers=headers, json=email_data)
        
        if response.status_code in [200, 202]:
            print(f"SUCCESS: Email sent successfully! Status code: {response.status_code}")
            print("Check your inbox for the test email.")
            return True
        else:
            print(f"FAILED: Failed to send email. Status code: {response.status_code}")
            print(f"Response: {response.text}")
            return False
            
    except Exception as e:
        print(f"ERROR: Error sending email: {str(e)}")
        return False

if __name__ == "__main__":
    success = send_test_email()
    sys.exit(0 if success else 1)
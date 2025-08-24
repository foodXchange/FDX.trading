import os
import smtplib
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart
from datetime import datetime
import json

def get_sendgrid_key():
    """Get SendGrid API key from Azure Key Vault or environment"""
    # Try to get from environment first
    api_key = os.environ.get('SENDGRID_API_KEY')
    
    if not api_key:
        # Try to read from the app configuration
        print("Checking for SendGrid API key in Azure Key Vault...")
        
        # Use Azure CLI to get the secret
        import subprocess
        try:
            result = subprocess.run(
                ['az', 'keyvault', 'secret', 'show', '--vault-name', 'fdx-kv-poland', '--name', 'SendGridApiKey'],
                capture_output=True,
                text=True
            )
            if result.returncode == 0:
                secret_data = json.loads(result.stdout)
                api_key = secret_data.get('value')
                print("✅ API key retrieved from Azure Key Vault")
            else:
                print("❌ Failed to get API key from Key Vault")
        except Exception as e:
            print(f"❌ Error accessing Key Vault: {e}")
    
    return api_key

def test_smtp_mode(api_key, to_email="udi@fdx.trading"):
    """Test sending email via SMTP"""
    print("\n" + "=" * 60)
    print("TESTING SMTP MODE")
    print("=" * 60)
    
    if not api_key:
        print("❌ No API key available for SMTP")
        return False
    
    try:
        # SendGrid SMTP settings
        smtp_server = "smtp.sendgrid.net"
        smtp_port = 587
        smtp_username = "apikey"  # This is always 'apikey' for SendGrid
        smtp_password = api_key
        
        # Create message
        msg = MIMEMultipart('alternative')
        msg['From'] = "FoodX Platform <udi@fdx.trading>"
        msg['To'] = to_email
        msg['Subject'] = f"SMTP Test - Magic Link Login - {datetime.now().strftime('%H:%M:%S')}"
        
        # Create the HTML content
        html_content = f"""
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; background: #f5f5f5; }}
                .container {{ max-width: 600px; margin: 50px auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; margin: -30px -30px 30px -30px; }}
                .button {{ display: inline-block; padding: 15px 30px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; text-align: center; color: #888; font-size: 12px; }}
            </style>
        </head>
        <body>
            <div class="container">
                <div class="header">
                    <h1>FoodX Trading Platform</h1>
                </div>
                <h2>SMTP Mode Test</h2>
                <p>This email was sent using <strong>SMTP relay</strong> via SendGrid.</p>
                <p>Your magic link for testing:</p>
                <a href="http://localhost:5193/account/magic-link-callback?token=test-smtp-{datetime.now().timestamp()}" class="button">
                    Sign In with Magic Link (SMTP Test)
                </a>
                <p>Time sent: {datetime.now()}</p>
                <div class="footer">
                    <p>This is a test email sent via SMTP mode</p>
                    <p>FoodX Trading Platform - B2B Food Trading</p>
                </div>
            </div>
        </body>
        </html>
        """
        
        plain_content = f"""
        FoodX Trading Platform
        SMTP Mode Test
        
        This email was sent using SMTP relay via SendGrid.
        
        Your magic link: http://localhost:5193/account/magic-link-callback?token=test-smtp-{datetime.now().timestamp()}
        
        Time sent: {datetime.now()}
        """
        
        # Attach parts
        part1 = MIMEText(plain_content, 'plain')
        part2 = MIMEText(html_content, 'html')
        msg.attach(part1)
        msg.attach(part2)
        
        # Send email
        print(f"Connecting to SMTP server: {smtp_server}:{smtp_port}")
        with smtplib.SMTP(smtp_server, smtp_port) as server:
            server.starttls()
            print("Authenticating...")
            server.login(smtp_username, smtp_password)
            print(f"Sending email to {to_email}...")
            server.send_message(msg)
            print(f"✅ SMTP email sent successfully to {to_email}")
            return True
            
    except Exception as e:
        print(f"❌ SMTP error: {e}")
        return False

def test_api_mode(api_key, to_email="udi@fdx.trading"):
    """Test sending email via SendGrid API"""
    print("\n" + "=" * 60)
    print("TESTING API MODE")
    print("=" * 60)
    
    if not api_key:
        print("❌ No API key available for API mode")
        return False
    
    try:
        import urllib.request
        import urllib.error
        
        # SendGrid API endpoint
        url = "https://api.sendgrid.net/v3/mail/send"
        
        # Create the email data
        data = {
            "personalizations": [{
                "to": [{"email": to_email}],
                "subject": f"API Test - Magic Link Login - {datetime.now().strftime('%H:%M:%S')}"
            }],
            "from": {
                "email": "udi@fdx.trading",
                "name": "FoodX Platform"
            },
            "content": [
                {
                    "type": "text/plain",
                    "value": f"FoodX Trading Platform\nAPI Mode Test\n\nThis email was sent using SendGrid Web API.\n\nYour magic link: http://localhost:5193/account/magic-link-callback?token=test-api-{datetime.now().timestamp()}\n\nTime sent: {datetime.now()}"
                },
                {
                    "type": "text/html",
                    "value": f"""
                    <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                        <div style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center;">
                            <h1>FoodX Trading Platform</h1>
                        </div>
                        <div style="padding: 30px;">
                            <h2>API Mode Test</h2>
                            <p>This email was sent using <strong>SendGrid Web API</strong>.</p>
                            <p>Your magic link for testing:</p>
                            <a href="http://localhost:5193/account/magic-link-callback?token=test-api-{datetime.now().timestamp()}" 
                               style="display: inline-block; padding: 15px 30px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; border-radius: 5px;">
                                Sign In with Magic Link (API Test)
                            </a>
                            <p>Time sent: {datetime.now()}</p>
                        </div>
                    </div>
                    """
                }
            ]
        }
        
        # Create the request
        req = urllib.request.Request(url)
        req.add_header('Authorization', f'Bearer {api_key}')
        req.add_header('Content-Type', 'application/json')
        
        # Send the request
        print(f"Sending API request to SendGrid...")
        print(f"To: {to_email}")
        
        json_data = json.dumps(data).encode('utf-8')
        response = urllib.request.urlopen(req, json_data)
        
        if response.status in [200, 202]:
            print(f"✅ API email sent successfully to {to_email}")
            print(f"Response status: {response.status}")
            return True
        else:
            print(f"❌ API request failed with status: {response.status}")
            return False
            
    except urllib.error.HTTPError as e:
        print(f"❌ API HTTP error: {e.code} - {e.reason}")
        if hasattr(e, 'read'):
            print(f"Response body: {e.read().decode()}")
        return False
    except Exception as e:
        print(f"❌ API error: {e}")
        return False

def main():
    print("=" * 60)
    print("SENDGRID DUAL MODE TEST")
    print("=" * 60)
    print(f"Time: {datetime.now()}")
    
    # Get API key
    api_key = get_sendgrid_key()
    
    if not api_key:
        print("\n❌ Cannot proceed without SendGrid API key")
        print("\nTo fix this:")
        print("1. Ensure you're logged into Azure CLI: az login")
        print("2. Or set environment variable: set SENDGRID_API_KEY=your-key-here")
        return
    
    print(f"\n✅ API key found (starts with: {api_key[:10]}...)")
    
    # Test both modes
    smtp_success = test_smtp_mode(api_key)
    api_success = test_api_mode(api_key)
    
    # Summary
    print("\n" + "=" * 60)
    print("TEST SUMMARY")
    print("=" * 60)
    print(f"SMTP Mode: {'✅ SUCCESS' if smtp_success else '❌ FAILED'}")
    print(f"API Mode:  {'✅ SUCCESS' if api_success else '❌ FAILED'}")
    
    if smtp_success or api_success:
        print("\n✅ At least one mode is working!")
        print("The dual-mode email service will automatically use the working mode.")
    else:
        print("\n❌ Both modes failed. Please check:")
        print("1. SendGrid API key is valid")
        print("2. Domain authentication is verified")
        print("3. Sender email is verified")
    
    print("\nCheck your email inbox for the test messages!")

if __name__ == "__main__":
    main()
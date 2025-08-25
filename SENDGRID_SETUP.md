# SendGrid Configuration for FoodX Platform

## Issue
Magic link emails are not being sent because SendGrid API key is not configured.

## Current Status
- SendGrid service is implemented and working
- When API key is missing, emails are logged locally to `magic-links.txt`
- The system is currently in fallback mode (logging only)

## Required Configuration

### Option 1: Configure SendGrid API Key in Development (Recommended for Testing)

1. Get your SendGrid API key from https://app.sendgrid.com/settings/api_keys
2. Update `FoodX.Admin/appsettings.Development.json`:
```json
{
  "SendGrid": {
    "ApiKey": "YOUR_ACTUAL_SENDGRID_API_KEY",
    "FromEmail": "udi@fdx.trading",
    "FromName": "FoodX Platform",
    "UseApi": true,
    "UseSmtp": false
  },
  "SendGridApiKey": "YOUR_ACTUAL_SENDGRID_API_KEY"
}
```

### Option 2: Configure in Azure Key Vault (Production)

1. Login to Azure CLI:
```bash
az login
```

2. Add SendGrid API key to Key Vault:
```bash
az keyvault secret set --vault-name fdx-kv-poland --name SendGridApiKey --value "YOUR_ACTUAL_SENDGRID_API_KEY"
```

### Option 3: Use Environment Variable (Alternative)

Set environment variable:
```bash
# Windows
set SendGridApiKey=YOUR_ACTUAL_SENDGRID_API_KEY

# PowerShell
$env:SendGridApiKey="YOUR_ACTUAL_SENDGRID_API_KEY"
```

## Verify SendGrid Setup

### Prerequisites for SendGrid
1. Create SendGrid account at https://sendgrid.com
2. Verify sender domain or email address
3. Create API key with "Mail Send" permissions

### Domain Authentication (Important!)
For `udi@fdx.trading` to work as sender:
1. Go to SendGrid Settings > Sender Authentication
2. Authenticate the domain `fdx.trading`
3. Add required DNS records to your domain

### Testing
After configuration, test with:
```bash
python test_sendgrid_direct.py
```

## Current Workaround (Development Mode)

While SendGrid is not configured, magic links are saved to:
- File: `FoodX.Admin/magic-links.txt`
- Console logs (when running in development)

You can manually copy the magic link URL from the log file to test login functionality.

## Security Notes
- Never commit API keys to source control
- Use Azure Key Vault for production
- Rotate API keys regularly
- Monitor SendGrid for unusual activity

## Support
- SendGrid Documentation: https://docs.sendgrid.com
- API Key Management: https://app.sendgrid.com/settings/api_keys
- Domain Authentication: https://app.sendgrid.com/settings/sender_auth
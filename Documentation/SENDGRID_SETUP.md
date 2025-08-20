# SendGrid Setup Guide for FoodX Trading Platform

## Quick Start

### 1. Get Your SendGrid API Key

1. Go to https://signup.sendgrid.com/ and create a free account
2. Verify your email address
3. Navigate to **Settings > API Keys**
4. Click **Create API Key**
5. Name it "FoodX Production" 
6. Select **Full Access**
7. Copy the API key (you'll only see it once!)

### 2. Configure Your Application

#### Option A: Environment Variable (Recommended for Production)
```bash
# Windows PowerShell
$env:SendGrid__ApiKey = "SG.your-api-key-here"

# Windows CMD
set SendGrid__ApiKey=SG.your-api-key-here

# Linux/Mac
export SendGrid__ApiKey="SG.your-api-key-here"
```

#### Option B: User Secrets (Recommended for Development)
```bash
cd C:\Users\fdxadmin\source\repos\FDX.trading\FoodX.Admin
dotnet user-secrets init
dotnet user-secrets set "SendGrid:ApiKey" "SG.your-api-key-here"
```

#### Option C: appsettings.json (NOT recommended - don't commit API keys!)
```json
{
  "SendGrid": {
    "ApiKey": "SG.your-api-key-here",
    "FromEmail": "noreply@fdx.trading",
    "FromName": "FoodX Trading Platform"
  }
}
```

### 3. Test Your Configuration

#### Using cURL:
```bash
curl -X GET https://api.sendgrid.com/v3/scopes \
  -H "Authorization: Bearer SG.your-api-key-here"
```

Expected response: List of scopes including `mail.send`

#### Using PowerShell:
```powershell
$headers = @{
    "Authorization" = "Bearer SG.your-api-key-here"
}
Invoke-RestMethod -Uri "https://api.sendgrid.com/v3/scopes" -Headers $headers
```

### 4. Send a Test Email

```bash
curl -X POST https://api.sendgrid.com/v3/mail/send \
  -H "Authorization: Bearer SG.your-api-key-here" \
  -H "Content-Type: application/json" \
  -d '{
    "personalizations": [{
      "to": [{"email": "udi@fdx.trading"}]
    }],
    "from": {"email": "noreply@fdx.trading"},
    "subject": "Test from FoodX",
    "content": [{
      "type": "text/plain",
      "value": "This is a test email from FoodX Trading Platform"
    }]
  }'
```

## Development Mode

When `SendGrid:ApiKey` is not set or empty, the application runs in **Development Mode**:
- Emails are NOT sent
- Magic links are saved to `magic-links.txt` file
- Full email content is logged to console

This is perfect for testing without using SendGrid credits!

## Production Setup

### 1. Domain Authentication (Optional but Recommended)

1. In SendGrid, go to **Settings > Sender Authentication**
2. Click **Authenticate Your Domain**
3. Follow the DNS setup instructions for `fdx.trading`
4. This improves deliverability significantly

### 2. Set Production Configuration

```json
{
  "SendGrid": {
    "ApiKey": "Use-Environment-Variable-Instead",
    "FromEmail": "noreply@fdx.trading",
    "FromName": "FoodX Trading"
  },
  "BaseUrl": "https://fdx.trading",
  "Email": {
    "Mode": "Production"
  }
}
```

### 3. Monitor Your Usage

- Free tier: 100 emails/day forever
- Track usage at: https://app.sendgrid.com/statistics

## Pricing Tiers

| Plan | Price | Emails/Month | Features |
|------|-------|--------------|----------|
| Free | $0 | 3,000 | Basic features |
| Essentials | $19.95 | 50,000 | Email validation, analytics |
| Pro | $89.95 | 100,000 | Dedicated IP, priority support |

## Troubleshooting

### API Key Not Working
- Ensure it starts with `SG.`
- Check it has `mail.send` scope
- Verify no extra spaces or quotes

### Emails Not Sending in Production
1. Check API key is set correctly
2. Verify sender email is authenticated
3. Check SendGrid dashboard for bounces/blocks
4. Review application logs for errors

### Rate Limits
- Free tier: 100 emails/day
- Paid tiers: 600 emails/minute

## Support

- SendGrid Docs: https://docs.sendgrid.com/
- API Reference: https://docs.sendgrid.com/api-reference/
- Status Page: https://status.sendgrid.com/

## Security Best Practices

1. **NEVER** commit API keys to Git
2. Use environment variables or Azure Key Vault
3. Rotate API keys regularly
4. Use IP whitelisting if possible
5. Monitor for unusual activity

## Testing Magic Links Locally

1. Leave `SendGrid:ApiKey` empty in appsettings.json
2. Run the application
3. Request a magic link
4. Check `magic-links.txt` file for the link
5. Copy and paste the link in browser

No SendGrid account needed for development!
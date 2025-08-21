#!/bin/bash
echo "Testing SendGrid API directly..."
API_KEY=$(az keyvault secret show --vault-name fdx-kv-poland --name SendGridApiKey --query "value" -o tsv)
echo "API Key retrieved from Key Vault: ${API_KEY:0:10}..."

echo "Sending test email..."
curl -X POST https://api.sendgrid.com/v3/mail/send \
  -H "Authorization: Bearer $API_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "personalizations": [{
      "to": [{"email": "udi@fdx.trading"}]
    }],
    "from": {
      "email": "udi@fdx.trading",
      "name": "FoodX Platform"
    },
    "subject": "Test Magic Link from FoodX",
    "content": [{
      "type": "text/html",
      "value": "<h2>Test Email</h2><p>If you receive this, SendGrid is working correctly!</p><p>This is a test of the magic link email system.</p>"
    }]
  }'
echo ""
echo "Email sent!"
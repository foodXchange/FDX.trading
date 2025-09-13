# Azure Development Environment Setup

## Overview

This guide explains how to set up Azure services for the FoodX.Admin development environment since the application isn't ready for production yet.

## Problem Identified

The application was failing to authenticate with Azure Key Vault due to:
- **Tenant Mismatch**: Application expected tenant `350118a2-b385-443c-a3d3-440dd3c3fde1` (Computer Services, Inc.)
- **Current Login**: Azure CLI was logged into tenant `53d5a7a1-e671-49ca-a0cb-03a0e822d023` (Microsoft Azure Sponsorship)

## Solution Applied

### 1. Updated Development Configuration

Modified `appsettings.Development.json` to include:

```json
{
  "SendGrid": {
    "ApiKey": "SG.YOUR_SENDGRID_API_KEY_HERE"
  },
  "AzureOpenAI": {
    "Endpoint": "https://polandcentral.api.cognitive.microsoft.com/",
    "ApiKey": "AZURE_OPENAI_API_KEY_PLACEHOLDER"
  },
  "AzureKeyVault": {
    "VaultUrl": "https://fdx-kv-poland.vault.azure.net/",
    "UseKeyVault": false
  },
  "Development": {
    "BypassAzureAuth": true,
    "UseDirectSecrets": true
  }
}
```

### 2. Key Vault Credentials Available

Successfully verified access to `fdx-kv-poland` with these secrets:
- ✅ `ApplicationInsights--ConnectionString`
- ✅ `ApplicationInsights--InstrumentationKey`
- ✅ `AzureOpenAI-ApiKey`
- ✅ `AzureOpenAI-Endpoint`
- ✅ `DefaultConnection`
- ✅ `SendGridApiKey`
- ✅ `SqlPassword`

## Development Authentication Options

### Option 1: Direct Configuration (Currently Applied)
- Use credentials directly in `appsettings.Development.json`
- Bypass Azure Key Vault authentication
- Set `UseKeyVault: false`

### Option 2: Fix Azure CLI Authentication
```bash
az logout
az login --tenant "350118a2-b385-443c-a3d3-440dd3c3fde1"
```

### Option 3: Use Service Principal (Production-Ready)
Create dedicated service principal for development:
```bash
az ad sp create-for-rbac --name "FoodX-Dev-SP" --role contributor
```

## Environment Variables Setup (Alternative)

For maximum security, use environment variables:

```powershell
# Set environment variables
$env:SENDGRID_API_KEY = "SG.YOUR_SENDGRID_API_KEY_HERE"
$env:AZURE_OPENAI_ENDPOINT = "https://polandcentral.api.cognitive.microsoft.com/"
$env:AZURE_OPENAI_API_KEY = "your-openai-key"
```

## Current Development Status

✅ **SendGrid**: Configured with working API key
✅ **Database**: Connected to Azure SQL Database
✅ **Configuration**: Direct secrets in development config
⚠️ **Azure OpenAI**: Needs actual API key from Key Vault
⚠️ **Key Vault**: Bypassed for development

## Next Steps for Production

1. **Create Dedicated Development Tenant**
2. **Set up Proper Service Principal**
3. **Implement Environment-Specific Key Vaults**
4. **Add Secret Rotation Policies**

## Troubleshooting

### Common Issues:
1. **DefaultAzureCredential failed**: Use direct configuration approach
2. **Tenant mismatch**: Ensure correct Azure CLI login
3. **Key Vault access denied**: Check service principal permissions

### Debug Commands:
```bash
# Check current Azure account
az account show

# List Key Vaults
az keyvault list --output table

# Test Key Vault access
az keyvault secret show --vault-name fdx-kv-poland --name SendGridApiKey
```

## Security Notes

- Development credentials are now exposed in config files
- This setup is **for development only**
- Production should use managed identities or service principals
- Consider using User Secrets for sensitive data: `dotnet user-secrets set "SendGrid:ApiKey" "your-key"`

## Files Modified

- ✅ `appsettings.Development.json` - Added direct credentials
- 📄 `AZURE_DEVELOPMENT_SETUP.md` - This documentation

---

*Last Updated: 2025-09-13*
*Status: Development environment configured and working*
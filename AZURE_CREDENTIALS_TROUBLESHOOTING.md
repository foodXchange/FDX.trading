# Azure Credentials Troubleshooting Guide

## Quick Reference

### 🔍 Current Status Check Commands
```bash
# Check Azure CLI login status
az account show

# List available Key Vaults
az keyvault list --output table

# Test specific secret access
az keyvault secret show --vault-name fdx-kv-poland --name SendGridApiKey --query "value" --output tsv

# Check application process
netstat -an | findstr ":5195"
```

## Common Issues & Solutions

### 1. DefaultAzureCredential Failed ❌

**Error**: `DefaultAzureCredential failed to retrieve a token from the included credentials`

**Root Cause**: Multiple authentication methods failed in sequence

**Solutions**:

#### ✅ Option A: Use Direct Configuration (Current Setup)
```json
// appsettings.Development.json
{
  "AzureKeyVault": {
    "UseKeyVault": false
  },
  "SendGrid": {
    "ApiKey": "SG.YOUR_SENDGRID_API_KEY_HERE"
  }
}
```

#### ✅ Option B: Fix Azure CLI Login
```bash
az logout
az login --tenant "350118a2-b385-443c-a3d3-440dd3c3fde1"
```

#### ✅ Option C: Use Environment Variables
```powershell
$env:AZURE_CLIENT_ID = "your-client-id"
$env:AZURE_CLIENT_SECRET = "your-client-secret"
$env:AZURE_TENANT_ID = "350118a2-b385-443c-a3d3-440dd3c3fde1"
```

### 2. Tenant Mismatch Error ❌

**Error**: `User account 'foodz-x@hotmail.com' from identity provider 'live.com' does not exist in tenant 'Computer Services, Inc.'`

**Expected Tenant**: `350118a2-b385-443c-a3d3-440dd3c3fde1`
**Current Tenant**: `53d5a7a1-e671-49ca-a0cb-03a0e822d023`

**Solution**:
```bash
# Login to correct tenant
az logout
az login --tenant "350118a2-b385-443c-a3d3-440dd3c3fde1" --scope "https://vault.azure.net/.default"
```

### 3. Visual Studio Credential Failed ❌

**Error**: `VisualStudioCredential authentication failed`

**Solutions**:
1. Clear Visual Studio token cache
2. Re-login through Visual Studio Account settings
3. Or bypass using direct configuration (recommended for dev)

### 4. Managed Identity Not Available ❌

**Error**: `ManagedIdentityCredential authentication unavailable`

**Explanation**: Normal in development - Managed Identity only works in Azure-hosted resources

**Solution**: Use alternative authentication method (already handled)

### 5. Azure CLI Authentication Failed ❌

**Error**: `AzureCliCredential authentication failed`

**Diagnostic Steps**:
```bash
# Check current login
az account show

# Check available subscriptions
az account list --output table

# Re-authenticate
az login --use-device-code
```

## Development Environment Setup

### Current Working Configuration

✅ **Status**: Development environment working
✅ **Database**: Connected to Azure SQL
✅ **SendGrid**: API key configured
✅ **Application**: Running on http://localhost:5195

### Key Vault Secrets Available
- ✅ ApplicationInsights--ConnectionString
- ✅ ApplicationInsights--InstrumentationKey
- ✅ AzureOpenAI-ApiKey
- ✅ AzureOpenAI-Endpoint
- ✅ DefaultConnection
- ✅ SendGridApiKey
- ✅ SqlPassword

## Authentication Methods Priority

Azure DefaultAzureCredential tries these methods in order:

1. **EnvironmentCredential** ❌ (Not configured)
2. **WorkloadIdentityCredential** ❌ (Not configured)
3. **ManagedIdentityCredential** ❌ (Not available locally)
4. **VisualStudioCredential** ❌ (Tenant mismatch)
5. **VisualStudioCodeCredential** ❌ (Package missing)
6. **AzureCliCredential** ❌ (Tenant mismatch)
7. **AzurePowerShellCredential** ❌ (Module not installed)

**Current Solution**: Bypass all with direct configuration ✅

## Recommended Development Workflow

### For Daily Development
1. Use current setup with `UseKeyVault: false`
2. Credentials in `appsettings.Development.json`
3. Application works without Azure authentication

### For Testing Azure Integration
1. Login to correct tenant: `az login --tenant "350118a2-b385-443c-a3d3-440dd3c3fde1"`
2. Set `UseKeyVault: true`
3. Test Key Vault connectivity

### For Production Deployment
1. Use Managed Identity
2. Configure service principal
3. Implement proper secret management

## Security Considerations

### ⚠️ Development Risks
- Secrets exposed in configuration files
- No secret rotation
- Full production access

### 🔒 Mitigation Strategies
1. Use `.gitignore` for sensitive configs
2. Consider User Secrets: `dotnet user-secrets set "SendGrid:ApiKey" "value"`
3. Implement environment-specific Key Vaults

## Diagnostic Commands

```bash
# Full Azure status check
az account show --output table
az keyvault list --output table
az keyvault secret list --vault-name fdx-kv-poland --output table

# Application status check
netstat -an | findstr ":5195"
curl http://localhost:5195

# Process check
tasklist | findstr dotnet
```

## Emergency Recovery

If everything fails:
1. Stop all dotnet processes: `taskkill /f /im dotnet.exe`
2. Clear Azure cache: `az logout && az cache purge`
3. Use minimal config with hardcoded credentials
4. Restart application: `dotnet run --launch-profile Development`

## Contact & Support

- **Azure Subscription**: Microsoft Azure Sponsorship (53d5a7a1-e671-49ca-a0cb-03a0e822d023)
- **Key Vault**: fdx-kv-poland (polandcentral)
- **Database**: fdx-sql-prod.database.windows.net

---

*Last Updated: 2025-09-13*
*Environment: Development - Not Production Ready*
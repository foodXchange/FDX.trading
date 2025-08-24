# CI/CD Pipeline Setup Guide - FDX Trading Platform

## Overview
This guide provides instructions for setting up automated CI/CD pipelines for the FDX Trading Platform using either Azure DevOps or GitHub Actions.

## 🚀 Quick Start

### Option 1: Azure DevOps Pipeline

1. **Create Azure DevOps Project**
   - Navigate to https://dev.azure.com
   - Create new project: "FDX-Trading"
   - Import repository from GitHub

2. **Create Service Connection**
   ```bash
   # In Azure DevOps: Project Settings > Service Connections
   # Create new Azure Resource Manager connection
   Name: FDX-Azure-Connection
   Subscription: [Your Azure Subscription]
   Resource Group: fdx-dotnet-rg
   ```

3. **Create Variable Group**
   - Navigate to Pipelines > Library
   - Create variable group: "FDX-Production-Secrets"
   - Link to Azure Key Vault: `fdx-kv-poland`
   - Add variables:
     - SqlAdminUser
     - SqlAdminPassword
     - StorageAccountKey

4. **Create Pipeline**
   - Pipelines > New Pipeline
   - Select repository
   - Choose "Existing Azure Pipelines YAML file"
   - Path: `/azure-pipelines.yml`

5. **Create Environments**
   - Pipelines > Environments
   - Create "FDX-Staging" and "FDX-Production"
   - Add approval gates for Production

### Option 2: GitHub Actions

1. **Set up GitHub Secrets**
   ```bash
   # Repository Settings > Secrets and variables > Actions
   
   # Required secrets:
   AZURE_CREDENTIALS       # Service principal JSON
   STORAGE_ACCOUNT_KEY     # For database backups
   ```

2. **Create Azure Service Principal**
   ```bash
   az ad sp create-for-rbac \
     --name "FDX-GitHub-Actions" \
     --role contributor \
     --scopes /subscriptions/{subscription-id}/resourceGroups/fdx-dotnet-rg \
     --sdk-auth
   ```

3. **Configure Environments**
   - Settings > Environments
   - Create "Staging" and "Production"
   - Add protection rules for Production:
     - Required reviewers
     - Deployment branches: main only

## 📁 Pipeline Files Structure

```
FDX.trading/
├── azure-pipelines.yml          # Azure DevOps pipeline
├── .github/
│   └── workflows/
│       └── ci-cd.yml            # GitHub Actions workflow
├── deploy/
│   ├── build.ps1                # Build script
│   └── deploy.ps1               # Deployment script
└── tests/
    └── smoke-tests.ps1          # Post-deployment tests
```

## 🔧 Pipeline Features

### Security Scanning
- **Credential scanning** - Detects exposed secrets
- **Vulnerability scanning** - Checks NuGet packages
- **Code analysis** - Static security analysis
- **Dependency checking** - Identifies vulnerable dependencies

### Deployment Strategy
- **Blue-Green deployment** - Using staging slots
- **Zero-downtime** - Slot swapping for production
- **Automatic rollback** - On deployment failure
- **Database backup** - Before each production deployment

### Monitoring Integration
- **Application Insights** - Automatic configuration
- **Health checks** - Post-deployment validation
- **Smoke tests** - Verify critical functionality
- **Deployment tracking** - Full audit trail

## 🔐 Security Configuration

### Key Vault Integration
All secrets are stored in Azure Key Vault (`fdx-kv-poland`):
- Database connection strings
- Application Insights keys
- SendGrid API keys
- Storage account keys

### Required Permissions
Service principal needs:
- **Contributor** on resource group
- **Key Vault Secrets User** on Key Vault
- **Web App Contributor** on App Services

## 📊 Deployment Stages

### 1. Build Stage
- Restore NuGet packages
- Build all projects
- Run unit tests
- Security scanning
- Create artifacts

### 2. Staging Deployment
- Deploy to staging slots
- Run database migrations
- Update app settings
- Execute smoke tests

### 3. Production Deployment
- Manual approval required
- Database backup
- Deploy to production
- Verify Application Insights
- Run production tests

## 🎯 App Service Configuration

### Required App Services
```bash
# Production
fdx-admin-prod
fdx-buyer-prod
fdx-supplier-prod
fdx-marketplace-prod

# Staging
fdx-admin-staging
fdx-buyer-staging
fdx-supplier-staging
fdx-marketplace-staging
```

### Deployment Slots
Each production app service should have:
- **staging** slot for blue-green deployment
- Auto-swap disabled (manual approval required)

## 📈 Monitoring & Alerts

### Application Insights
- Resource: `fdx-app-insights`
- Automatic integration via pipeline
- Performance tracking enabled
- Custom metrics configured

### Deployment Notifications
- Email alerts on deployment success/failure
- Slack/Teams integration available
- GitHub PR comments (GitHub Actions)

## 🚨 Troubleshooting

### Common Issues

1. **Key Vault Access Denied**
   ```bash
   az keyvault set-policy \
     --name fdx-kv-poland \
     --spn <service-principal-id> \
     --secret-permissions get list
   ```

2. **Deployment Slot Not Found**
   ```bash
   az webapp deployment slot create \
     --name fdx-admin-prod \
     --resource-group fdx-dotnet-rg \
     --slot staging
   ```

3. **Database Migration Failed**
   - Check connection string in Key Vault
   - Verify SQL firewall rules
   - Review migration logs

## 📝 Manual Deployment (Emergency)

If pipelines are unavailable:
```powershell
# Build locally
.\deploy\build.ps1 -Configuration Release

# Deploy manually
.\deploy\deploy.ps1 -Environment Production

# Run tests
.\tests\smoke-tests.ps1 -Environment Production
```

## ✅ Checklist Before First Deployment

- [ ] Azure DevOps/GitHub repository connected
- [ ] Service principal created with proper permissions
- [ ] Key Vault secrets configured
- [ ] App Services created with staging slots
- [ ] Database backup storage configured
- [ ] Application Insights resource created
- [ ] Email alerts configured
- [ ] Approval gates set up for production

## 📞 Support

- **Pipeline Issues**: Check Azure DevOps/GitHub Actions logs
- **Deployment Failures**: Review smoke test results
- **Security Alerts**: Check credential scan reports
- **Performance Issues**: Monitor Application Insights

---

**Last Updated**: August 24, 2025
**Version**: 1.0.0
# Azure Credentials and Configuration

## Azure Subscription
- **Subscription Name**: Microsoft Azure Sponsorship
- **Subscription ID**: 88931ed0-52df-42fb-a09c-e024c9586f8a
- **Tenant ID**: 53d5a7a1-e671-49ca-a0cb-03a0e822d023

## Azure Resource Group
- **Resource Group Name**: fdx-dotnet-rg
- **Region**: (To be confirmed)

## Microsoft Entra ID (Azure Active Directory)
- **Admin User**: Udi Stryk
- **Admin Email**: foodz-x@hotmail.com
- **Admin Object ID**: 57b7b3d6-90d3-41de-90ba-a4667b260695
- **Authentication Type**: Microsoft Entra MFA

## Azure CLI Authentication
```bash
# Login to Azure
az login

# Set subscription
az account set --subscription "88931ed0-52df-42fb-a09c-e024c9586f8a"

# Verify current account
az account show

# Get access token for SQL Database
az account get-access-token --resource https://database.windows.net/
```

## Current Firewall Rules
The following IP addresses are allowed to access the SQL Server:
- AllowAllAzure: 0.0.0.0 - 0.0.0.0 (Azure services)
- AllowMyIP: 79.177.131.138
- AllowMyIP2: 5.102.238.214
- AllowVM: 74.248.158.104
- CurrentIP: 31.154.96.164
- LocalDev: 217.132.156.166
- MyCurrentIP: 20.215.250.188

## Adding New IP to Firewall
```bash
# Get current IP
MYIP=$(curl -s ifconfig.me)

# Add firewall rule
az sql server firewall-rule create \
  --resource-group fdx-dotnet-rg \
  --server fdx-sql-prod \
  --name allow-new-ip \
  --start-ip-address $MYIP \
  --end-ip-address $MYIP
```
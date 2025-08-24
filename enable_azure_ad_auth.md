# URGENT: Password Rotation & Azure AD Authentication Setup

## Current Security Issue
The SQL password `FoodX@2024!Secure#Trading` was exposed in source control and needs immediate action.

## Immediate Actions Required

### Option 1: Rotate SQL Password (Requires SQL Admin Access)
1. **New secure password has been generated** and stored in Azure Key Vault
2. **Execute on Azure SQL Server** (requires admin credentials):
   ```sql
   -- Connect to master database
   -- Run: update_sql_password.sql
   ```
3. Contact your Azure SQL admin (`fdxadmin`) to execute the password change

### Option 2: Switch to Azure AD Authentication (Recommended)
This eliminates passwords entirely and is more secure:

1. **Enable Azure AD authentication on SQL Server**:
   ```bash
   az sql server ad-admin create \
     --resource-group fdx-dotnet-rg \
     --server fdx-sql-prod \
     --display-name "FDX Admin" \
     --object-id <your-azure-ad-object-id>
   ```

2. **Create Azure AD user in database**:
   ```sql
   CREATE USER [foodxapp@yourdomain.com] FROM EXTERNAL PROVIDER;
   ALTER ROLE db_datareader ADD MEMBER [foodxapp@yourdomain.com];
   ALTER ROLE db_datawriter ADD MEMBER [foodxapp@yourdomain.com];
   ```

3. **Update connection string** (already configured in Development):
   ```
   Server=tcp:fdx-sql-prod.database.windows.net,1433;
   Database=fdxdb;
   Authentication=Active Directory Default;
   Encrypt=True;
   TrustServerCertificate=False;
   ```

## Verification Steps
1. Test new connection: `dotnet run --environment Development`
2. Check application logs for successful database connection
3. Verify all test users can still login

## Security Best Practices Going Forward
- ✅ All secrets in Azure Key Vault
- ✅ Use Azure AD authentication (no passwords)
- ✅ Enable SQL audit logging
- ✅ Implement database backup strategy
- ✅ Regular security reviews

## Contact
If you need assistance with SQL admin access, contact your Azure administrator immediately.
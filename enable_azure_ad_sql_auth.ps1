# Enable Azure AD Authentication for SQL Database
Write-Host "Configuring Azure AD Authentication for SQL..." -ForegroundColor Yellow

$resourceGroup = "fdx-dotnet-rg"
$serverName = "fdx-sql-prod"

# Get current user's Azure AD details
Write-Host "Getting Azure AD user details..." -ForegroundColor Green
$currentUser = az ad signed-in-user show --query "{displayName:displayName, objectId:id, upn:userPrincipalName}" -o json | ConvertFrom-Json

Write-Host "Setting up Azure AD admin for SQL Server..." -ForegroundColor Green
Write-Host "  User: $($currentUser.displayName)" -ForegroundColor Cyan
Write-Host "  UPN: $($currentUser.upn)" -ForegroundColor Cyan

# Set Azure AD admin for SQL Server
az sql server ad-admin create `
    --resource-group $resourceGroup `
    --server $serverName `
    --display-name $currentUser.displayName `
    --object-id $currentUser.objectId | Out-Null

Write-Host "`nAzure AD Authentication enabled!" -ForegroundColor Green

# Create SQL script to add Azure AD users
$sqlScript = @"
-- Run this in the fdxdb database as Azure AD admin
-- Create Azure AD user for the application
CREATE USER [fdxadmin@fdx.trading] FROM EXTERNAL PROVIDER;

-- Grant necessary permissions
ALTER ROLE db_datareader ADD MEMBER [fdxadmin@fdx.trading];
ALTER ROLE db_datawriter ADD MEMBER [fdxadmin@fdx.trading];
ALTER ROLE db_ddladmin ADD MEMBER [fdxadmin@fdx.trading];

-- Create user for managed identity (if using App Service)
-- CREATE USER [your-app-service-name] FROM EXTERNAL PROVIDER;
-- ALTER ROLE db_datareader ADD MEMBER [your-app-service-name];
-- ALTER ROLE db_datawriter ADD MEMBER [your-app-service-name];

PRINT 'Azure AD users configured successfully';
"@

$sqlScript | Out-File -FilePath "../create_azure_ad_sql_users.sql" -Encoding UTF8

Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Connect to the database using Azure AD authentication" -ForegroundColor White
Write-Host "2. Execute create_azure_ad_sql_users.sql to create database users" -ForegroundColor White
Write-Host "3. Update connection strings to use 'Authentication=Active Directory Default'" -ForegroundColor White

Write-Host "`nConnection string format:" -ForegroundColor Cyan
Write-Host "Server=tcp:$serverName.database.windows.net,1433;Database=fdxdb;Authentication=Active Directory Default;Encrypt=True;" -ForegroundColor White
# Azure CLI Authentication Setup Script
# This will help reduce the frequency of authentication prompts

Write-Host "Setting up Azure Authentication..." -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green

# 1. Login to Azure CLI with your credentials
Write-Host "`n1. Logging in to Azure CLI..." -ForegroundColor Yellow
az login --username foodz-x@hotmail.com

# 2. Set the default subscription (if you have multiple)
Write-Host "`n2. Setting default subscription..." -ForegroundColor Yellow
az account list --output table
# az account set --subscription "Your-Subscription-Name-or-ID"

# 3. Configure Azure CLI to not prompt as often
Write-Host "`n3. Configuring Azure CLI settings..." -ForegroundColor Yellow
az config set core.login_experience_v2=off
az config set core.no_color=false
az config set core.disable_confirm_prompt=true

# 4. Cache credentials for SQL Database
Write-Host "`n4. Setting up SQL Database connection..." -ForegroundColor Yellow
$env:AZURE_SQL_CONNECTION_STRING = "Server=tcp:fdx-sql-prod.database.windows.net,1433;Initial Catalog=fdxdb;Persist Security Info=False;User ID=foodz-x@hotmail.com;Password=Foodz2025;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# 5. Store credentials in Windows Credential Manager (more secure)
Write-Host "`n5. Storing credentials securely..." -ForegroundColor Yellow
cmdkey /add:fdx-sql-prod.database.windows.net /user:foodz-x@hotmail.com /pass:Foodz2025

# 6. Create a connection config file
Write-Host "`n6. Creating connection config..." -ForegroundColor Yellow
$config = @{
    AzureAccount = "foodz-x@hotmail.com"
    SQLServer = "fdx-sql-prod.database.windows.net"
    Database = "fdxdb"
    AuthType = "SQLAuth"
}
$config | ConvertTo-Json | Out-File -FilePath ".\azure_config.json"

Write-Host "`n=================================" -ForegroundColor Green
Write-Host "Azure Authentication Setup Complete!" -ForegroundColor Green
Write-Host "`nNOTE: You'll still need to authenticate periodically for security," -ForegroundColor Yellow
Write-Host "but this setup will reduce the frequency of prompts." -ForegroundColor Yellow
Write-Host "`nYour credentials are stored securely in Windows Credential Manager." -ForegroundColor Cyan
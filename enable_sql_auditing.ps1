# Enable SQL Database Auditing for fdx-sql-prod
Write-Host "Enabling SQL Database Auditing..." -ForegroundColor Yellow

# Create storage account for audit logs if it doesn't exist
$resourceGroup = "fdx-dotnet-rg"
$storageAccountName = "fdxsqlauditlogs"
$location = "polandcentral"
$serverName = "fdx-sql-prod"
$databaseName = "fdxdb"

# Check if storage account exists
$storageExists = az storage account show --name $storageAccountName --resource-group $resourceGroup 2>$null
if (!$storageExists) {
    Write-Host "Creating storage account for audit logs..." -ForegroundColor Green
    az storage account create `
        --name $storageAccountName `
        --resource-group $resourceGroup `
        --location $location `
        --sku Standard_LRS `
        --kind StorageV2 | Out-Null
}

# Get storage account key
$storageKey = az storage account keys list `
    --resource-group $resourceGroup `
    --account-name $storageAccountName `
    --query '[0].value' -o tsv

# Enable server-level auditing
Write-Host "Configuring server-level auditing..." -ForegroundColor Green
az sql server audit-policy update `
    --resource-group $resourceGroup `
    --server $serverName `
    --state Enabled `
    --storage-account $storageAccountName `
    --storage-key $storageKey `
    --retention-days 90 | Out-Null

# Enable database-level auditing
Write-Host "Configuring database-level auditing..." -ForegroundColor Green
az sql db audit-policy update `
    --resource-group $resourceGroup `
    --server $serverName `
    --name $databaseName `
    --state Enabled `
    --storage-account $storageAccountName `
    --storage-key $storageKey `
    --retention-days 90 `
    --actions "DATABASE_AUTHENTICATION_FAILED_GROUP" "SUCCESSFUL_DATABASE_AUTHENTICATION_GROUP" "FAILED_DATABASE_AUTHENTICATION_GROUP" | Out-Null

Write-Host "SQL Auditing enabled successfully!" -ForegroundColor Green
Write-Host "Audit logs will be stored in: $storageAccountName" -ForegroundColor Cyan
Write-Host "Retention period: 90 days" -ForegroundColor Cyan
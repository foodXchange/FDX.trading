# Set up Application Insights for FDX Trading Platform
Write-Host "Setting up Application Insights..." -ForegroundColor Yellow

$resourceGroup = "fdx-dotnet-rg"
$appInsightsName = "fdx-app-insights"
$location = "polandcentral"
$workspaceName = "fdx-log-analytics"

# Create Log Analytics Workspace
Write-Host "Creating Log Analytics Workspace..." -ForegroundColor Green
$workspace = az monitor log-analytics workspace create `
    --resource-group $resourceGroup `
    --workspace-name $workspaceName `
    --location $location `
    --query id -o tsv

# Create Application Insights resource
Write-Host "Creating Application Insights resource..." -ForegroundColor Green
$appInsights = az monitor app-insights component create `
    --app $appInsightsName `
    --location $location `
    --resource-group $resourceGroup `
    --workspace $workspace `
    --application-type web `
    --kind web `
    --query instrumentationKey -o tsv

Write-Host "Application Insights created successfully!" -ForegroundColor Green
Write-Host "Instrumentation Key: $appInsights" -ForegroundColor Cyan

# Store the instrumentation key in Key Vault
Write-Host "Storing Instrumentation Key in Key Vault..." -ForegroundColor Green
az keyvault secret set `
    --vault-name "fdx-kv-poland" `
    --name "ApplicationInsights--InstrumentationKey" `
    --value $appInsights | Out-Null

# Get connection string
$connectionString = az monitor app-insights component show `
    --app $appInsightsName `
    --resource-group $resourceGroup `
    --query connectionString -o tsv

# Store connection string in Key Vault
az keyvault secret set `
    --vault-name "fdx-kv-poland" `
    --name "ApplicationInsights--ConnectionString" `
    --value $connectionString | Out-Null

Write-Host "`nApplication Insights configured!" -ForegroundColor Green
Write-Host "Resource Name: $appInsightsName" -ForegroundColor Cyan
Write-Host "Workspace: $workspaceName" -ForegroundColor Cyan
Write-Host "Secrets stored in Azure Key Vault" -ForegroundColor Cyan
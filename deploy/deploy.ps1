# Deployment Script for FDX Trading Platform
# Handles deployment to Azure App Services with zero-downtime deployment

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("Staging", "Production")]
    [string]$Environment,
    
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroup = "fdx-dotnet-rg",
    
    [Parameter(Mandatory=$false)]
    [string]$KeyVaultName = "fdx-kv-poland"
)

Write-Host "Starting deployment to $Environment environment..." -ForegroundColor Green

# Function to deploy an app service
function Deploy-AppService {
    param(
        [string]$AppName,
        [string]$PackagePath,
        [string]$SlotName = $null
    )
    
    Write-Host "Deploying to $AppName..." -ForegroundColor Cyan
    
    try {
        if ($SlotName) {
            # Deploy to slot
            az webapp deployment source config-zip `
                --resource-group $ResourceGroup `
                --name $AppName `
                --slot $SlotName `
                --src $PackagePath
        } else {
            # Deploy to production
            az webapp deployment source config-zip `
                --resource-group $ResourceGroup `
                --name $AppName `
                --src $PackagePath
        }
        
        Write-Host "  ✓ Deployment successful" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "  ✗ Deployment failed: $_" -ForegroundColor Red
        return $false
    }
}

# Function to swap deployment slots
function Swap-DeploymentSlot {
    param(
        [string]$AppName,
        [string]$SourceSlot = "staging"
    )
    
    Write-Host "Swapping $SourceSlot slot to production for $AppName..." -ForegroundColor Cyan
    
    try {
        az webapp deployment slot swap `
            --resource-group $ResourceGroup `
            --name $AppName `
            --slot $SourceSlot
        
        Write-Host "  ✓ Slot swap successful" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "  ✗ Slot swap failed: $_" -ForegroundColor Red
        return $false
    }
}

# Function to update app settings from Key Vault
function Update-AppSettings {
    param(
        [string]$AppName
    )
    
    Write-Host "Updating app settings for $AppName..." -ForegroundColor Cyan
    
    try {
        # Get secrets from Key Vault
        $connectionString = az keyvault secret show `
            --vault-name $KeyVaultName `
            --name "DefaultConnection" `
            --query value -o tsv
        
        $appInsightsKey = az keyvault secret show `
            --vault-name $KeyVaultName `
            --name "ApplicationInsights--InstrumentationKey" `
            --query value -o tsv
        
        # Update app settings
        az webapp config appsettings set `
            --resource-group $ResourceGroup `
            --name $AppName `
            --settings `
                "ConnectionStrings__DefaultConnection=$connectionString" `
                "ApplicationInsights__InstrumentationKey=$appInsightsKey" `
                "ASPNETCORE_ENVIRONMENT=$Environment"
        
        Write-Host "  ✓ App settings updated" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "  ✗ Failed to update app settings: $_" -ForegroundColor Red
        return $false
    }
}

# Main deployment logic
$deploymentSuccess = $true

# Define app services based on environment
if ($Environment -eq "Production") {
    $appServices = @(
        @{ Name = "fdx-admin-prod"; Package = "Admin.zip" },
        @{ Name = "fdx-buyer-prod"; Package = "Buyer.zip" },
        @{ Name = "fdx-supplier-prod"; Package = "Supplier.zip" },
        @{ Name = "fdx-marketplace-prod"; Package = "Marketplace.zip" }
    )
} else {
    $appServices = @(
        @{ Name = "fdx-admin-staging"; Package = "Admin.zip" },
        @{ Name = "fdx-buyer-staging"; Package = "Buyer.zip" },
        @{ Name = "fdx-supplier-staging"; Package = "Supplier.zip" },
        @{ Name = "fdx-marketplace-staging"; Package = "Marketplace.zip" }
    )
}

# Deploy each app service
foreach ($app in $appServices) {
    Write-Host "`nDeploying $($app.Name)..." -ForegroundColor Yellow
    
    # Update app settings
    if (-not (Update-AppSettings -AppName $app.Name)) {
        $deploymentSuccess = $false
        continue
    }
    
    # Deploy package
    if ($Environment -eq "Production") {
        # Deploy to staging slot first
        if (Deploy-AppService -AppName $app.Name -PackagePath $app.Package -SlotName "staging") {
            # Swap to production
            if (-not (Swap-DeploymentSlot -AppName $app.Name)) {
                $deploymentSuccess = $false
            }
        } else {
            $deploymentSuccess = $false
        }
    } else {
        # Direct deployment for staging environment
        if (-not (Deploy-AppService -AppName $app.Name -PackagePath $app.Package)) {
            $deploymentSuccess = $false
        }
    }
    
    # Restart app service
    Write-Host "Restarting $($app.Name)..." -ForegroundColor Cyan
    az webapp restart --resource-group $ResourceGroup --name $app.Name
}

# Run database migrations if needed
Write-Host "`nChecking for database migrations..." -ForegroundColor Yellow
try {
    $migrationScript = "dotnet ef database update"
    Write-Host "Running migrations..." -ForegroundColor Cyan
    Invoke-Expression $migrationScript
    Write-Host "  ✓ Migrations completed" -ForegroundColor Green
}
catch {
    Write-Host "  ✗ Migration failed: $_" -ForegroundColor Red
    $deploymentSuccess = $false
}

# Summary
Write-Host "`n========================================" -ForegroundColor White
Write-Host "DEPLOYMENT SUMMARY - $Environment" -ForegroundColor White
Write-Host "========================================" -ForegroundColor White

if ($deploymentSuccess) {
    Write-Host "Deployment COMPLETED successfully! ✓" -ForegroundColor Green
    
    if ($Environment -eq "Production") {
        Write-Host "`nProduction URLs:" -ForegroundColor Cyan
        Write-Host "  Admin: https://admin.fdx.trading" -ForegroundColor White
        Write-Host "  Buyer: https://buyer.fdx.trading" -ForegroundColor White
        Write-Host "  Supplier: https://supplier.fdx.trading" -ForegroundColor White
        Write-Host "  Marketplace: https://marketplace.fdx.trading" -ForegroundColor White
    }
    
    exit 0
} else {
    Write-Host "Deployment FAILED! ✗" -ForegroundColor Red
    Write-Host "Please check the logs and try again." -ForegroundColor Yellow
    exit 1
}
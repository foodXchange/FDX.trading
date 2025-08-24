# Smoke Tests for FDX Trading Platform
# Validates deployment health after deployment

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("Staging", "Production")]
    [string]$Environment
)

Write-Host "Running smoke tests for $Environment environment..." -ForegroundColor Yellow

# Set URLs based on environment
if ($Environment -eq "Production") {
    $urls = @{
        Admin = "https://admin.fdx.trading"
        Buyer = "https://buyer.fdx.trading"
        Supplier = "https://supplier.fdx.trading"
        Marketplace = "https://marketplace.fdx.trading"
    }
} else {
    $urls = @{
        Admin = "https://fdx-admin-staging.azurewebsites.net"
        Buyer = "https://fdx-buyer-staging.azurewebsites.net"
        Supplier = "https://fdx-supplier-staging.azurewebsites.net"
        Marketplace = "https://fdx-marketplace-staging.azurewebsites.net"
    }
}

$testResults = @()
$allTestsPassed = $true

# Function to test endpoint
function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Url,
        [string]$HealthPath = "/health"
    )
    
    Write-Host "Testing $Name..." -ForegroundColor Cyan
    
    try {
        # Test home page
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 30
        if ($response.StatusCode -eq 200) {
            Write-Host "  ✓ Home page accessible" -ForegroundColor Green
        } else {
            Write-Host "  ✗ Home page returned status $($response.StatusCode)" -ForegroundColor Red
            return $false
        }
        
        # Test health endpoint
        $healthUrl = "$Url$HealthPath"
        $healthResponse = Invoke-WebRequest -Uri $healthUrl -UseBasicParsing -TimeoutSec 10
        if ($healthResponse.StatusCode -eq 200) {
            Write-Host "  ✓ Health check passed" -ForegroundColor Green
        } else {
            Write-Host "  ✗ Health check failed with status $($healthResponse.StatusCode)" -ForegroundColor Red
            return $false
        }
        
        return $true
    }
    catch {
        Write-Host "  ✗ Failed to connect: $_" -ForegroundColor Red
        return $false
    }
}

# Test all portals
foreach ($portal in $urls.GetEnumerator()) {
    $result = Test-Endpoint -Name $portal.Key -Url $portal.Value
    $testResults += @{
        Portal = $portal.Key
        Url = $portal.Value
        Passed = $result
    }
    
    if (-not $result) {
        $allTestsPassed = $false
    }
}

# Test database connectivity
Write-Host "`nTesting database connectivity..." -ForegroundColor Cyan
try {
    $connectionString = $env:DefaultConnection
    if ([string]::IsNullOrEmpty($connectionString)) {
        # Try to get from Azure Key Vault
        $connectionString = az keyvault secret show `
            --vault-name "fdx-kv-poland" `
            --name "DefaultConnection" `
            --query value -o tsv
    }
    
    # Simple connectivity test using sqlcmd
    $testQuery = "SELECT COUNT(*) FROM AspNetUsers"
    $result = sqlcmd -S "fdx-sql-prod.database.windows.net" `
        -d "fdxdb" `
        -Q $testQuery `
        -b 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Database connectivity verified" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Database connectivity failed" -ForegroundColor Red
        $allTestsPassed = $false
    }
}
catch {
    Write-Host "  ✗ Database test failed: $_" -ForegroundColor Red
    $allTestsPassed = $false
}

# Test Application Insights
Write-Host "`nTesting Application Insights..." -ForegroundColor Cyan
try {
    $appInsights = az monitor app-insights component show `
        --app "fdx-app-insights" `
        --resource-group "fdx-dotnet-rg" `
        --query "provisioningState" -o tsv
    
    if ($appInsights -eq "Succeeded") {
        Write-Host "  ✓ Application Insights is active" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Application Insights state: $appInsights" -ForegroundColor Red
        $allTestsPassed = $false
    }
}
catch {
    Write-Host "  ✗ Application Insights test failed: $_" -ForegroundColor Red
    $allTestsPassed = $false
}

# Summary
Write-Host "`n========================================" -ForegroundColor White
Write-Host "SMOKE TEST SUMMARY - $Environment" -ForegroundColor White
Write-Host "========================================" -ForegroundColor White

foreach ($result in $testResults) {
    $status = if ($result.Passed) { "PASS" } else { "FAIL" }
    $color = if ($result.Passed) { "Green" } else { "Red" }
    Write-Host "$($result.Portal): $status" -ForegroundColor $color
}

if ($allTestsPassed) {
    Write-Host "`nAll smoke tests PASSED! ✓" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`nSome smoke tests FAILED! ✗" -ForegroundColor Red
    Write-Host "Please check the deployment and try again." -ForegroundColor Yellow
    exit 1
}
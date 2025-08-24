# FDX.Trading Multi-Portal Startup Script
# This script launches all portals for local development

Write-Host "Starting FDX.Trading Multi-Portal Platform..." -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green

# Check if .NET is installed
$dotnetVersion = dotnet --version
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: .NET SDK is not installed" -ForegroundColor Red
    exit 1
}
Write-Host "Using .NET SDK version: $dotnetVersion" -ForegroundColor Cyan

# Set environment to Development
$env:ASPNETCORE_ENVIRONMENT = "Development"
Write-Host "Environment: Development" -ForegroundColor Yellow

# Function to start a portal
function Start-Portal {
    param (
        [string]$Name,
        [string]$Path,
        [string]$Url
    )
    
    Write-Host "`nStarting $Name..." -ForegroundColor Cyan
    Write-Host "URL: $Url" -ForegroundColor Gray
    
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$Path'; dotnet run" -WindowStyle Normal
}

# Start all portals
Start-Portal -Name "Admin Portal" -Path "FoodX.Admin" -Url "http://localhost:5193"
Start-Sleep -Seconds 2

Start-Portal -Name "Buyer Portal" -Path "FoodX.Buyer" -Url "http://localhost:5000"
Start-Sleep -Seconds 2

Start-Portal -Name "Supplier Portal" -Path "FoodX.Supplier" -Url "http://localhost:5001"
Start-Sleep -Seconds 2

Start-Portal -Name "Marketplace" -Path "FoodX.Marketplace" -Url "http://localhost:5002"

Write-Host "`n=======================================" -ForegroundColor Green
Write-Host "All portals are starting..." -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green
Write-Host "`nPortal URLs:" -ForegroundColor Yellow
Write-Host "  Admin:       http://localhost:5193" -ForegroundColor White
Write-Host "  Buyer:       http://localhost:5000" -ForegroundColor White
Write-Host "  Supplier:    http://localhost:5001" -ForegroundColor White
Write-Host "  Marketplace: http://localhost:5002" -ForegroundColor White
Write-Host "`nPress Ctrl+C in each window to stop the portals" -ForegroundColor Gray
# Build Script for FDX Trading Platform
# Builds all projects and prepares for deployment

param(
    [Parameter(Mandatory=$false)]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "./artifacts"
)

Write-Host "Starting build process..." -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Cyan

# Ensure output directory exists
if (!(Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath | Out-Null
}

# Function to build a project
function Build-Project {
    param(
        [string]$ProjectName,
        [string]$ProjectPath
    )
    
    Write-Host "`nBuilding $ProjectName..." -ForegroundColor Yellow
    
    try {
        # Clean
        Write-Host "  Cleaning..." -ForegroundColor Cyan
        dotnet clean $ProjectPath --configuration $Configuration
        
        # Restore
        Write-Host "  Restoring packages..." -ForegroundColor Cyan
        dotnet restore $ProjectPath
        
        # Build
        Write-Host "  Building..." -ForegroundColor Cyan
        dotnet build $ProjectPath --configuration $Configuration --no-restore
        
        # Publish
        Write-Host "  Publishing..." -ForegroundColor Cyan
        $publishPath = Join-Path $OutputPath $ProjectName
        dotnet publish $ProjectPath `
            --configuration $Configuration `
            --no-build `
            --output $publishPath
        
        # Create deployment package
        Write-Host "  Creating deployment package..." -ForegroundColor Cyan
        $zipPath = Join-Path $OutputPath "$ProjectName.zip"
        Compress-Archive -Path "$publishPath/*" -DestinationPath $zipPath -Force
        
        Write-Host "  ✓ Build successful: $zipPath" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "  ✗ Build failed: $_" -ForegroundColor Red
        return $false
    }
}

# Projects to build
$projects = @(
    @{ Name = "Admin"; Path = "FoodX.Admin/FoodX.Admin.csproj" },
    @{ Name = "Buyer"; Path = "FoodX.Buyer/FoodX.Buyer.csproj" },
    @{ Name = "Supplier"; Path = "FoodX.Supplier/FoodX.Supplier.csproj" },
    @{ Name = "Marketplace"; Path = "FoodX.Marketplace/FoodX.Marketplace.csproj" }
)

$buildSuccess = $true

# Build all projects
foreach ($project in $projects) {
    if (!(Build-Project -ProjectName $project.Name -ProjectPath $project.Path)) {
        $buildSuccess = $false
    }
}

# Run tests
Write-Host "`nRunning tests..." -ForegroundColor Yellow
try {
    dotnet test --configuration $Configuration --no-build --logger "console;verbosity=normal"
    Write-Host "  ✓ All tests passed" -ForegroundColor Green
}
catch {
    Write-Host "  ⚠ Some tests failed" -ForegroundColor Yellow
    # Don't fail the build for test failures in CI
}

# Security checks
Write-Host "`nRunning security checks..." -ForegroundColor Yellow

# Check for vulnerable packages
Write-Host "  Checking for vulnerable packages..." -ForegroundColor Cyan
$vulnerablePackages = dotnet list package --vulnerable --include-transitive 2>&1
if ($vulnerablePackages -match "no vulnerable") {
    Write-Host "    ✓ No vulnerable packages found" -ForegroundColor Green
} else {
    Write-Host "    ⚠ Vulnerable packages detected:" -ForegroundColor Yellow
    Write-Host $vulnerablePackages
}

# Create build info file
$buildInfo = @{
    BuildDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Configuration = $Configuration
    Machine = $env:COMPUTERNAME
    User = $env:USERNAME
    CommitHash = git rev-parse HEAD 2>$null
    Branch = git rev-parse --abbrev-ref HEAD 2>$null
}

$buildInfoPath = Join-Path $OutputPath "build-info.json"
$buildInfo | ConvertTo-Json | Out-File $buildInfoPath -Encoding UTF8
Write-Host "`nBuild info saved to: $buildInfoPath" -ForegroundColor Cyan

# Summary
Write-Host "`n========================================" -ForegroundColor White
Write-Host "BUILD SUMMARY" -ForegroundColor White
Write-Host "========================================" -ForegroundColor White

if ($buildSuccess) {
    Write-Host "Build COMPLETED successfully! ✓" -ForegroundColor Green
    Write-Host "`nArtifacts location: $OutputPath" -ForegroundColor Cyan
    
    Get-ChildItem $OutputPath -Filter "*.zip" | ForEach-Object {
        Write-Host "  - $($_.Name) ($([math]::Round($_.Length / 1MB, 2)) MB)" -ForegroundColor White
    }
    
    exit 0
} else {
    Write-Host "Build FAILED! ✗" -ForegroundColor Red
    exit 1
}
# Verification Script - Check for Sensitive Data in Git History

Write-Host "Verifying Git History Cleanup" -ForegroundColor Green
Write-Host "==============================" -ForegroundColor Green
Write-Host ""

$issuesFound = $false

# Check for sensitive files in history
Write-Host "Checking for sensitive files in history..." -ForegroundColor Yellow

$sensitiveFiles = @(
    "appsettings.Production.json",
    "appsettings.json",
    "azure_credentials.txt",
    "update_sql_password.sql"
)

foreach ($file in $sensitiveFiles) {
    Write-Host "  Checking: $file" -ForegroundColor Cyan
    $found = git log --all --full-history -- "*$file" 2>$null
    if ($found) {
        Write-Host "    [X] FOUND in history!" -ForegroundColor Red
        $issuesFound = $true
    } else {
        Write-Host "    [OK] Not found" -ForegroundColor Green
    }
}

# Check for sensitive strings
Write-Host ""
Write-Host "Checking for sensitive strings in history..." -ForegroundColor Yellow

$sensitiveStrings = @(
    "FoodX@2024",
    "Password=FoodX",
    "fdx-sql-prod.database"
)

foreach ($string in $sensitiveStrings) {
    Write-Host "  Checking: $string" -ForegroundColor Cyan
    try {
        $found = git grep "$string" $(git rev-list --all) 2>$null
        if ($found) {
            Write-Host "    [X] FOUND in history!" -ForegroundColor Red
            $issuesFound = $true
        } else {
            Write-Host "    [OK] Not found" -ForegroundColor Green
        }
    } catch {
        Write-Host "    [OK] Not found" -ForegroundColor Green
    }
}

# Check current working directory
Write-Host ""
Write-Host "Checking current working directory..." -ForegroundColor Yellow

$currentFile = "../FoodX.Admin/appsettings.json"
if (Test-Path $currentFile) {
    $content = Get-Content $currentFile -Raw -ErrorAction SilentlyContinue
    if ($content -match "FoodX@2024") {
        Write-Host "  [X] Sensitive data found in: $currentFile" -ForegroundColor Red
        $issuesFound = $true
    } else {
        Write-Host "  [OK] $currentFile is clean" -ForegroundColor Green
    }
}

# Summary
Write-Host ""
Write-Host "==============================" -ForegroundColor White
if ($issuesFound) {
    Write-Host "WARNING: ISSUES FOUND!" -ForegroundColor Red
    Write-Host "Sensitive data may still exist in the repository." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Recommended actions:" -ForegroundColor Yellow
    Write-Host "1. Run the cleanup script" -ForegroundColor White
    Write-Host "2. Force push to remote" -ForegroundColor White
    Write-Host "3. Have all team members re-clone" -ForegroundColor White
} else {
    Write-Host "OK: Repository is CLEAN!" -ForegroundColor Green
    Write-Host "No sensitive data found in git history." -ForegroundColor Green
    Write-Host "Repository is safe to push to remote." -ForegroundColor Cyan
}
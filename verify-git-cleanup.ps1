# Verification Script - Check for Sensitive Data in Git History
# Run this after cleaning to ensure sensitive data is removed

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
        Write-Host "    ✗ FOUND in history!" -ForegroundColor Red
        $issuesFound = $true
    } else {
        Write-Host "    ✓ Not found" -ForegroundColor Green
    }
}

# Check for sensitive strings
Write-Host "`nChecking for sensitive strings in history..." -ForegroundColor Yellow

$sensitiveStrings = @(
    "FoodX@2024",
    "Password=FoodX",
    "fdx-sql-prod.database.windows.net.*Password",
    "foodxapp.*Password"
)

foreach ($string in $sensitiveStrings) {
    Write-Host "  Checking: $string" -ForegroundColor Cyan
    try {
        $found = git grep -E "$string" $(git rev-list --all) 2>$null
        if ($found) {
            Write-Host "    ✗ FOUND in history!" -ForegroundColor Red
            $issuesFound = $true
        } else {
            Write-Host "    ✓ Not found" -ForegroundColor Green
        }
    } catch {
        Write-Host "    ✓ Not found" -ForegroundColor Green
    }
}

# Check current working directory
Write-Host "`nChecking current working directory..." -ForegroundColor Yellow

$currentFiles = @(
    "FoodX.Admin/appsettings.json",
    "azure_credentials.txt",
    "update_sql_password.sql"
)

foreach ($file in $currentFiles) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw -ErrorAction SilentlyContinue
        if ($content -match "FoodX@2024|Password=.*Secure.*Trading") {
            Write-Host "  X Sensitive data found in: $file" -ForegroundColor Red
            $issuesFound = $true
        } else {
            Write-Host "  OK $file is clean" -ForegroundColor Green
        }
    }
}

# Check repository size
Write-Host "`nRepository statistics:" -ForegroundColor Yellow
git count-objects -v -H

# Summary
Write-Host "`n==============================" -ForegroundColor White
if ($issuesFound) {
    Write-Host "WARNING: ISSUES FOUND!" -ForegroundColor Red
    Write-Host "Sensitive data may still exist in the repository." -ForegroundColor Yellow
    Write-Host "`nRecommended actions:" -ForegroundColor Yellow
    Write-Host "1. Run the cleanup script: .\clean_git_history.ps1" -ForegroundColor White
    Write-Host "2. Force push to remote: git push --force --all" -ForegroundColor White
    Write-Host "3. Have all team members re-clone the repository" -ForegroundColor White
    exit 1
} else {
    Write-Host "OK Repository is CLEAN!" -ForegroundColor Green
    Write-Host "No sensitive data found in git history." -ForegroundColor Green
    Write-Host "Repository is safe to push to remote." -ForegroundColor Cyan
    exit 0
}
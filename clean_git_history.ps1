# Git History Cleanup Script for FDX Trading
# Removes sensitive data from git history permanently

Write-Host "==================================================" -ForegroundColor Red
Write-Host "WARNING: This will rewrite git history!" -ForegroundColor Red
Write-Host "All team members will need to re-clone the repo!" -ForegroundColor Red
Write-Host "==================================================" -ForegroundColor Red
Write-Host ""

$confirm = Read-Host "Are you sure you want to proceed? (yes/no)"
if ($confirm -ne "yes") {
    Write-Host "Operation cancelled." -ForegroundColor Yellow
    exit 0
}

# Create backup branch
$backupBranch = "backup-before-clean-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
Write-Host "Creating backup branch: $backupBranch" -ForegroundColor Yellow
git checkout -b $backupBranch
git push origin $backupBranch

# Switch back to main
git checkout main

# Files containing sensitive data
$sensitiveFiles = @(
    "FoodX.Admin/appsettings.json",
    "FoodX.Admin/appsettings.Production.json",
    "FoodX.Buyer/appsettings.json",
    "FoodX.Supplier/appsettings.json",
    "FoodX.Marketplace/appsettings.json",
    "azure_credentials.txt",
    "*.log",
    "**/*.bak"
)

Write-Host "`nFiles to be cleaned from history:" -ForegroundColor Cyan
foreach ($file in $sensitiveFiles) {
    Write-Host "  - $file" -ForegroundColor White
}

# Use BFG Repo-Cleaner (faster than filter-branch)
Write-Host "`nDownloading BFG Repo-Cleaner..." -ForegroundColor Yellow
$bfgUrl = "https://repo1.maven.org/maven2/com/madgag/bfg/1.14.0/bfg-1.14.0.jar"
$bfgPath = "./bfg.jar"

if (!(Test-Path $bfgPath)) {
    Invoke-WebRequest -Uri $bfgUrl -OutFile $bfgPath
}

# Create file with sensitive strings to remove
$passwordsFile = "./passwords.txt"
@"
FoodX@2024!Secure#Trading
Password=FoodX@2024!Secure#Trading
Server=tcp:fdx-sql-prod.database.windows.net,1433;Database=fdxdb;User Id=foodxapp;Password=FoodX@2024!Secure#Trading
"@ | Out-File $passwordsFile -Encoding UTF8

Write-Host "`nRemoving sensitive strings from history..." -ForegroundColor Yellow
java -jar $bfgPath --replace-text $passwordsFile

Write-Host "`nRemoving sensitive files from history..." -ForegroundColor Yellow
foreach ($file in $sensitiveFiles) {
    Write-Host "  Removing: $file" -ForegroundColor Cyan
    java -jar $bfgPath --delete-files $file --no-blob-protection
}

# Clean up git repository
Write-Host "`nCleaning up git repository..." -ForegroundColor Yellow
git reflog expire --expire=now --all
git gc --prune=now --aggressive

# Verify cleanup
Write-Host "`nVerifying cleanup..." -ForegroundColor Yellow
$searchResults = git log --all --full-history -- "*appsettings.json" 2>$null
if ($searchResults) {
    Write-Host "  ⚠ Warning: Some sensitive files may still be in history" -ForegroundColor Yellow
} else {
    Write-Host "  ✓ Sensitive files removed from history" -ForegroundColor Green
}

# Search for password strings
$passwordFound = git grep "FoodX@2024" $(git rev-list --all) 2>$null
if ($passwordFound) {
    Write-Host "  ⚠ Warning: Password string may still exist in history" -ForegroundColor Yellow
} else {
    Write-Host "  ✓ Password strings removed from history" -ForegroundColor Green
}

# Clean up temporary files
Remove-Item $passwordsFile -Force -ErrorAction SilentlyContinue
Remove-Item $bfgPath -Force -ErrorAction SilentlyContinue

Write-Host "`n==================================================" -ForegroundColor Green
Write-Host "Git history cleanup completed!" -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Green
Write-Host ""
Write-Host "IMPORTANT NEXT STEPS:" -ForegroundColor Red
Write-Host "1. Review the changes carefully" -ForegroundColor Yellow
Write-Host "2. Force push to remote repository:" -ForegroundColor Yellow
Write-Host "   git push origin --force --all" -ForegroundColor Cyan
Write-Host "   git push origin --force --tags" -ForegroundColor Cyan
Write-Host "3. Notify all team members to re-clone the repository" -ForegroundColor Yellow
Write-Host "4. Delete old clones from all machines" -ForegroundColor Yellow
Write-Host "5. Update CI/CD pipelines if needed" -ForegroundColor Yellow
Write-Host ""
Write-Host "Backup branch created: $backupBranch" -ForegroundColor Green
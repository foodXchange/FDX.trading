# Safer Git History Cleanup using git filter-repo
# Alternative method that's more reliable

Write-Host "Git History Cleanup - Safer Method" -ForegroundColor Green
Write-Host "===================================" -ForegroundColor Green

# Check if git-filter-repo is installed
$filterRepoInstalled = Get-Command git-filter-repo -ErrorAction SilentlyContinue
if (!$filterRepoInstalled) {
    Write-Host "Installing git-filter-repo..." -ForegroundColor Yellow
    pip install git-filter-repo
}

# Create backup
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$backupDir = "../FDX.trading.backup.$timestamp"

Write-Host "`nCreating backup at: $backupDir" -ForegroundColor Yellow
Copy-Item -Path "." -Destination $backupDir -Recurse -Force

# Create list of files to remove
$filesToRemove = @"
FoodX.Admin/appsettings.json
FoodX.Admin/appsettings.Production.json
FoodX.Buyer/appsettings.json
FoodX.Supplier/appsettings.json
FoodX.Marketplace/appsettings.json
azure_credentials.txt
"@

$filesToRemove | Out-File "./files-to-remove.txt" -Encoding UTF8

# Create list of sensitive strings to replace
$stringsToReplace = @"
regex:Password=.*==>[REMOVED]
regex:FoodX@2024.*==>[REMOVED]
literal:Server=tcp:fdx-sql-prod.database.windows.net,1433;Database=fdxdb;User Id=foodxapp;Password=FoodX@2024!Secure#Trading;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;==>Server=[REMOVED]
"@

$stringsToReplace | Out-File "./strings-to-replace.txt" -Encoding UTF8

Write-Host "`nRemoving sensitive files from history..." -ForegroundColor Yellow
git filter-repo --invert-paths --paths-from-file ./files-to-remove.txt

Write-Host "`nReplacing sensitive strings in history..." -ForegroundColor Yellow
git filter-repo --replace-text ./strings-to-replace.txt

# Clean up
Remove-Item "./files-to-remove.txt" -Force
Remove-Item "./strings-to-replace.txt" -Force

Write-Host "`n✓ Git history cleaned!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Review changes" -ForegroundColor White
Write-Host "2. Add new remote: git remote add origin <url>" -ForegroundColor White
Write-Host "3. Force push: git push origin --force --all" -ForegroundColor White
Write-Host "`nBackup saved at: $backupDir" -ForegroundColor Cyan
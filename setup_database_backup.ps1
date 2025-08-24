# Configure Automated Database Backups for Azure SQL
Write-Host "Configuring Database Backup Policy..." -ForegroundColor Yellow

$resourceGroup = "fdx-dotnet-rg"
$serverName = "fdx-sql-prod"
$databaseName = "fdxdb"

# Configure Short-term retention (Point-in-time restore)
Write-Host "Setting up short-term backup retention (35 days)..." -ForegroundColor Green
az sql db str-policy set `
    --resource-group $resourceGroup `
    --server $serverName `
    --name $databaseName `
    --retention-days 35 `
    --diff-backup 12 | Out-Null

# Configure Long-term retention (LTR)
Write-Host "Setting up long-term backup retention..." -ForegroundColor Green
az sql db ltr-policy set `
    --resource-group $resourceGroup `
    --server $serverName `
    --name $databaseName `
    --weekly-retention "P4W" `
    --monthly-retention "P12M" `
    --yearly-retention "P5Y" `
    --week-of-year 1 | Out-Null

# Create an immediate backup
Write-Host "Creating an immediate backup..." -ForegroundColor Green
$backupName = "fdxdb-manual-backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
az sql db ltr-backup create `
    --resource-group $resourceGroup `
    --server $serverName `
    --name $databaseName `
    --backup-name $backupName | Out-Null

# List current backups
Write-Host "`nCurrent backup configuration:" -ForegroundColor Cyan
az sql db str-policy show `
    --resource-group $resourceGroup `
    --server $serverName `
    --name $databaseName `
    --query "{RetentionDays:retentionDays, DiffBackupHours:diffBackupIntervalInHours}" `
    -o table

Write-Host "`nBackup policy configured successfully!" -ForegroundColor Green
Write-Host "- Point-in-time restore: 35 days" -ForegroundColor Cyan
Write-Host "- Weekly backups retained: 4 weeks" -ForegroundColor Cyan
Write-Host "- Monthly backups retained: 12 months" -ForegroundColor Cyan
Write-Host "- Yearly backups retained: 5 years" -ForegroundColor Cyan
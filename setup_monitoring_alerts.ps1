# Set up Azure Monitor Alerts for Failed Login Attempts
Write-Host "Setting up monitoring and alerts..." -ForegroundColor Yellow

$resourceGroup = "fdx-dotnet-rg"
$serverName = "fdx-sql-prod"
$databaseName = "fdxdb"
$actionGroupName = "fdx-security-alerts"
$alertEmail = "udi@fdx.trading"

# Create action group for notifications
Write-Host "Creating action group for alerts..." -ForegroundColor Green
az monitor action-group create `
    --resource-group $resourceGroup `
    --name $actionGroupName `
    --short-name "FDXAlerts" `
    --email-receiver name="SecurityTeam" email-address=$alertEmail | Out-Null

# Get SQL Server resource ID
$sqlServerId = az sql server show `
    --resource-group $resourceGroup `
    --name $serverName `
    --query id -o tsv

# Create alert for failed login attempts
Write-Host "Creating alert for failed login attempts..." -ForegroundColor Green
az monitor metrics alert create `
    --name "SQL-Failed-Logins" `
    --resource-group $resourceGroup `
    --scopes $sqlServerId `
    --description "Alert when failed login attempts exceed threshold" `
    --condition "avg connection_failed_user_error > 5" `
    --window-size 5m `
    --evaluation-frequency 1m `
    --action $actionGroupName | Out-Null

# Create alert for successful logins from new locations
Write-Host "Creating alert for unusual login patterns..." -ForegroundColor Green
az monitor metrics alert create `
    --name "SQL-Unusual-Activity" `
    --resource-group $resourceGroup `
    --scopes $sqlServerId `
    --description "Alert on unusual database activity" `
    --condition "avg connection_successful > 100" `
    --window-size 15m `
    --evaluation-frequency 5m `
    --action $actionGroupName | Out-Null

# Create diagnostic settings to send logs to storage
Write-Host "Configuring diagnostic settings..." -ForegroundColor Green
az monitor diagnostic-settings create `
    --resource $sqlServerId `
    --name "SecurityMonitoring" `
    --storage-account "fdxsqlauditlogs" `
    --logs '[{"category": "SQLSecurityAuditEvents", "enabled": true, "retentionPolicy": {"days": 90, "enabled": true}}]' `
    --metrics '[{"category": "AllMetrics", "enabled": true, "retentionPolicy": {"days": 30, "enabled": true}}]' | Out-Null

Write-Host "`nMonitoring alerts configured successfully!" -ForegroundColor Green
Write-Host "Alerts will be sent to: $alertEmail" -ForegroundColor Cyan
Write-Host "Alert conditions:" -ForegroundColor Cyan
Write-Host "  - Failed logins > 5 in 5 minutes" -ForegroundColor White
Write-Host "  - Successful logins > 100 in 15 minutes" -ForegroundColor White
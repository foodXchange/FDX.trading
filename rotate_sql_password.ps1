# Generate secure password for SQL Server
function New-SecurePassword {
    $length = 32
    $chars = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%^&*'
    $password = -join (1..$length | ForEach-Object { $chars[(Get-Random -Maximum $chars.Length)] })
    return $password
}

$newPassword = New-SecurePassword
Write-Host "Generated new secure password" -ForegroundColor Green

# Store the new password in Azure Key Vault
Write-Host "Updating Azure Key Vault..." -ForegroundColor Yellow
az keyvault secret set --vault-name "fdx-kv-poland" --name "SqlPassword" --value $newPassword | Out-Null

# Update the connection string in Key Vault
$newConnectionString = "Server=tcp:fdx-sql-prod.database.windows.net,1433;Database=fdxdb;User Id=foodxapp;Password=$newPassword;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
az keyvault secret set --vault-name "fdx-kv-poland" --name "DefaultConnection" --value $newConnectionString | Out-Null

Write-Host "Azure Key Vault updated successfully" -ForegroundColor Green

# Create SQL script to update password
$sqlScript = @"
ALTER LOGIN [foodxapp] WITH PASSWORD = N'$newPassword';
"@

$sqlScript | Out-File -FilePath "update_sql_password.sql" -Encoding UTF8
Write-Host "SQL script created: update_sql_password.sql" -ForegroundColor Green
Write-Host "`nIMPORTANT: Execute the SQL script on the Azure SQL Server to complete password rotation" -ForegroundColor Red
Write-Host "Command: sqlcmd -S fdx-sql-prod.database.windows.net -d master -U <admin_user> -P <admin_password> -i update_sql_password.sql" -ForegroundColor Yellow
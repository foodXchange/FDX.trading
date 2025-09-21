# FoodX Platform Deployment and Monitoring Guide

## Architecture Overview

The FoodX platform consists of two main microservices:

- **FoodX.Admin** (Port 5195): Main administrative and buyer/supplier portal
- **FoodX.EmailService** (Port 5173): Dedicated email processing microservice

## Prerequisites

- .NET 9.0 SDK
- SQL Server (Azure SQL Database configured)
- SendGrid account for email services
- Azure Key Vault access (optional for development)

## Configuration

### Database Configuration

Both services use the Azure SQL Database:
- **Connection String**: `Server=tcp:fdx-sql-prod.database.windows.net,1433;Database=fdxdb;User Id=foodxapp;Password=FoodX@2024!Secure#Trading;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;`

### Service Endpoints

- **FoodX.Admin**: `http://localhost:5195`
- **FoodX.EmailService**: `http://localhost:5173`

## Build Commands

### Build All Services

```powershell
# Build FoodX.Admin
cd "C:\Users\fdxadmin\source\repos\FDX.trading\FoodX.Admin"
dotnet build --verbosity minimal

# Build FoodX.EmailService
cd "C:\Users\fdxadmin\source\repos\FDX.trading\FoodX.EmailService"
dotnet build --verbosity minimal
```

### Build Solution (Alternative)

```powershell
cd "C:\Users\fdxadmin\source\repos\FDX.trading"
dotnet build FDX.trading.sln --verbosity minimal
```

## Deployment Commands

### Start EmailService (Terminal 1)

```powershell
cd "C:\Users\fdxadmin\source\repos\FDX.trading\FoodX.EmailService"
dotnet run --environment Development
```

**Expected Output:**
```
[INFO] Azure Key Vault configured successfully
[INFO] Database initialized successfully
[INFO] Email Service starting on Development environment
[INFO] SendGrid configured: True
Now listening on: http://localhost:5173
Application started. Press Ctrl+C to shut down.
```

### Start FoodX.Admin (Terminal 2)

```powershell
cd "C:\Users\fdxadmin\source\repos\FDX.trading\FoodX.Admin"
dotnet run --environment Development
```

**Expected Output:**
```
[INFO] Azure Key Vault disabled - using direct configuration
[INFO] Using in-memory distributed cache
[DEBUG] Using connection string: Server=tcp:fdx-sql-prod.database.windows.net,1433;Database=fdxdb;***
[INFO] Azure OpenAI configured from Key Vault
[INFO] SendGrid configured with API key from Azure Key Vault
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5195
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

## Service Startup Order

1. **Start EmailService First** - The email service should be started before the Admin service
2. **Start FoodX.Admin Second** - The admin service includes health checks for the email service

## Health Monitoring

### Health Check Endpoints

| Service | Health Endpoint | Status |
|---------|----------------|--------|
| FoodX.Admin | `http://localhost:5195/health` | ✅ Healthy |
| FoodX.Admin | `http://localhost:5195/health/ready` | Readiness Check |
| FoodX.Admin | `http://localhost:5195/health/live` | Liveness Check |
| FoodX.EmailService | `http://localhost:5173/health` | ✅ Healthy |

### Health Dashboard

Access the comprehensive health monitoring dashboard:
- **URL**: `http://localhost:5195/health-ui`
- **Features**:
  - Real-time service status
  - Database connectivity monitoring
  - Email service availability
  - Historical health trends

### Manual Health Verification

```powershell
# Test EmailService
curl -s http://localhost:5173/health

# Test FoodX.Admin
curl -s http://localhost:5195/health

# Test Admin health dashboard
curl -s http://localhost:5195/health-ui
```

## Service Communication

The services are configured with proper CORS policies:

- **FoodX.EmailService** accepts connections from:
  - `http://localhost:5195` (FoodX.Admin)
  - `https://localhost:5195`
  - `http://localhost:5196` (Alternative port)
  - `https://localhost:5196`

## Troubleshooting

### Common Issues

1. **Email Service Database Error**
   - **Symptom**: `Invalid object name 'Emails'`
   - **Solution**: The EmailService uses a shared database; some tables may not exist yet
   - **Workaround**: The health check will still pass; email functionality may be limited

2. **Port Conflicts**
   - **Check if ports are in use**: `netstat -an | findstr ":5195"` or `netstat -an | findstr ":5173"`
   - **Kill processes**: Use Task Manager or `taskkill /F /PID <process_id>`

3. **Database Connection Issues**
   - **Verify connection string** in appsettings.Development.json
   - **Check Azure SQL firewall rules**
   - **Validate credentials**

4. **Azure Key Vault Issues**
   - **Development Mode**: Set `"AzureKeyVault:UseKeyVault": false` in appsettings.Development.json
   - **Authentication**: Ensure proper Azure credentials are configured

### Monitoring Commands

```powershell
# Check running processes
Get-Process | Where-Object {$_.ProcessName -like "*dotnet*"}

# Monitor port usage
netstat -an | findstr ":5195"
netstat -an | findstr ":5173"

# Test service connectivity
Test-NetConnection localhost -Port 5195
Test-NetConnection localhost -Port 5173
```

## Performance Monitoring

### Key Metrics to Monitor

1. **Response Times**
   - Health check response times < 200ms
   - API response times < 2 seconds

2. **Database Performance**
   - Connection pool utilization
   - Query execution times
   - Connection timeout errors

3. **Memory Usage**
   - Application memory consumption
   - Garbage collection frequency

4. **Email Service Metrics**
   - Email queue length
   - Send success rates
   - SendGrid API response times

## Backup and Recovery

### Configuration Backup

Important files to backup:
- `FoodX.Admin/appsettings.Development.json`
- `FoodX.EmailService/appsettings.Development.json`
- `FoodX.Admin/appsettings.json`
- `FoodX.EmailService/appsettings.json`

### Database Backup

The Azure SQL Database is configured with automatic backups. For manual backups:

```sql
-- Create database backup (run in SQL Server Management Studio)
BACKUP DATABASE fdxdb TO URL = 'https://fdxstorage.blob.core.windows.net/backups/fdxdb.bak'
```

## Security Considerations

1. **Connection Strings**: Stored in Azure Key Vault in production
2. **API Keys**: SendGrid keys managed through Azure Key Vault
3. **HTTPS**: Services support HTTPS endpoints
4. **CORS**: Properly configured for cross-origin requests
5. **Rate Limiting**: Implemented in FoodX.Admin service

## Support Contacts

- **Development Team**: Technical issues and feature requests
- **DevOps Team**: Deployment and infrastructure issues
- **Database Team**: SQL Server and data-related issues

---

*Last Updated: 2025-09-19*
*Version: 1.0*
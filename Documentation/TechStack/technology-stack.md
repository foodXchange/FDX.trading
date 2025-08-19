# Technology Stack Documentation

## Development Environment

### IDE and Tools
- **Primary IDE**: Visual Studio 2022
  - Version: Latest (17.x)
  - Workload: ASP.NET and web development
  - Extensions: Azure development tools

- **Database Management**: SQL Server Management Studio (SSMS)
  - Version: 19.x
  - Connection: Azure SQL Database
  - Authentication: Microsoft Entra MFA

- **Version Control**: Git
  - Repository: Local (C:\Users\fdxadmin\source\repos\FDX.trading)
  - Remote: (To be configured)

### Command Line Tools
- **Azure CLI**: Version 2.76.0
  - Python: 3.12.10
  - Installation: C:\Program Files\Microsoft SDKs\Azure\CLI2

- **SQL Command Line**: sqlcmd
  - Version: 15.0.1300.359
  - Driver: ODBC Driver 17 for SQL Server

- **.NET CLI**: dotnet
  - SDKs: 9.0.303, 9.0.304
  - Runtime: .NET 9.0

## Backend Technologies

### Framework
- **.NET 9.0**
  - Project Type: Blazor Server
  - Target Framework: net9.0
  - Language: C# 12
  - Nullable Reference Types: Enabled

### Web Framework
- **Blazor Server**
  - Rendering: Server-side
  - Real-time updates: SignalR
  - Pages location: Components/Pages/
  - Shared components: Components/

### UI Framework
- **MudBlazor**
  - Version: (Latest)
  - Material Design components
  - Responsive layout
  - Theme customization

### Database
- **Azure SQL Database**
  - Server: fdx-sql-prod.database.windows.net
  - Database: fdxdb
  - Authentication: SQL Auth / Microsoft Entra ID

### ORM
- **Entity Framework Core**
  - Approach: Code-First
  - Context: FdxDbContext
  - Migrations: Database/Schema/Migrations/
  - Models: Data/Entities.cs

## Frontend Technologies

### Markup
- **Razor Components** (.razor files)
  - Component-based architecture
  - Two-way data binding
  - Event handling

### Styling
- **CSS**
  - Custom styles: wwwroot/css/site.css
  - Font styles: wwwroot/css/fonts.css
  - Material Design (via MudBlazor)

### Fonts
- **Custom Fonts**:
  - Causten (18 variants)
  - David Libre (3 variants)
  - Roboto Serif (18 variants)

### Assets
- **Logos**: Food Xchange branding (14 variants)
- **Location**: wwwroot/Assets/

## Cloud Infrastructure

### Hosting Platform
- **Microsoft Azure**
  - Subscription: Microsoft Azure Sponsorship
  - Resource Group: fdx-dotnet-rg

### Services Used
- **Azure SQL Database**
  - Automatic backups
  - Geo-redundancy
  - Elastic scaling

- **Azure App Service** (Planned)
  - Hosting for Blazor application
  - Auto-scaling
  - CI/CD integration

### Security
- **Authentication**
  - Microsoft Entra ID (Azure AD)
  - Multi-Factor Authentication (MFA)
  - SQL Authentication (backup)

- **Connection Security**
  - TLS/SSL encryption required
  - IP firewall rules
  - Private endpoints (future)

## Package Management

### NuGet Packages
Key packages in use:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" />
<PackageReference Include="MudBlazor" />
<PackageReference Include="System.Data.SqlClient" Version="4.8.6" />
```

### Python Packages (for scripts)
- pyodbc: 5.2.0 (SQL connectivity)

## Architecture Patterns

### Design Patterns
- **Repository Pattern**: (To be implemented)
- **Dependency Injection**: Built-in DI container
- **Service Layer**: UserService.cs

### Project Structure
```
FDX.trading/
├── Components/           # Blazor components
│   ├── Pages/           # Page components
│   └── Layout/          # Layout components
├── Data/                # Data models
├── Database/            # Database scripts
│   └── Schema/          # SQL migrations
├── Documentation/       # Project documentation
├── Services/            # Business logic
├── wwwroot/            # Static files
├── Program.cs          # Application entry
└── appsettings.json    # Configuration
```

## Development Practices

### Coding Standards
- **Naming**: PascalCase for public, camelCase for private
- **Async/Await**: For all I/O operations
- **Nullable Types**: Enabled project-wide
- **Code Analysis**: Built-in analyzers

### Testing (To be implemented)
- **Unit Testing**: xUnit/NUnit
- **Integration Testing**: TestServer
- **UI Testing**: bUnit for Blazor

### Deployment
- **Local Development**: IIS Express / Kestrel
- **Production**: Azure App Service (planned)
- **Database Migrations**: EF Core migrations

## Browser Support
- **Modern Browsers**: Chrome, Edge, Firefox, Safari
- **WebSocket Support**: Required for SignalR
- **JavaScript**: Minimal usage (Blazor Server)

## Performance Considerations
- **Server-Side Rendering**: Reduced client load
- **SignalR**: Real-time UI updates
- **Connection Pooling**: EF Core optimization
- **Caching**: (To be implemented)
# FDX Trading Documentation

Welcome to the FDX Trading platform documentation. This comprehensive guide contains all the information needed to understand, develop, and maintain the FDX Trading B2B marketplace platform.

## 📁 Documentation Structure

### 🔐 [Credentials](./Credentials/)
Secure storage of all authentication and access information:
- **[Azure Credentials](./Credentials/azure-credentials.md)** - Azure subscription, resource groups, and CLI access
- **[Database Credentials](./Credentials/database-credentials.md)** - SQL Server connection strings and authentication methods
- **[GitHub Repository](./Credentials/github-repository.md)** - Repository details, clone commands, and Git operations

### 🗄️ [Database](./Database/)
Complete database documentation and schemas:
- **[Database Overview](./Database/database-overview.md)** - Azure SQL Database configuration and design patterns
- **[Schema Documentation](./Database/schema-documentation.md)** - Detailed table structures, relationships, and constraints

### 💻 [TechStack](./TechStack/)
Technology stack and development environment:
- **[Technology Stack](./TechStack/technology-stack.md)** - Complete list of technologies, frameworks, and tools used

### 📋 [ProjectPlan](./ProjectPlan/)
Vision, roadmap, and project planning:
- **[Project Vision](./ProjectPlan/project-vision.md)** - Business goals, target market, and long-term vision
- **[Development Roadmap](./ProjectPlan/development-roadmap.md)** - Phased development plan with timelines and milestones

## 🚀 Quick Start

1. **Environment Setup**
   - Install Visual Studio 2022 with ASP.NET workload
   - Install SQL Server Management Studio (SSMS)
   - Install Azure CLI
   - Clone the repository to `C:\Users\fdxadmin\source\repos\FDX.trading`

2. **Database Connection**
   - Use Microsoft Entra authentication (recommended)
   - Server: `fdx-sql-prod.database.windows.net`
   - Database: `fdxdb`
   - See [Database Credentials](./Credentials/database-credentials.md) for details

3. **Run the Application**
   ```bash
   cd FDX.trading
   dotnet run
   ```
   Navigate to: `https://localhost:5001`

## 📊 Current Project Status

### Completed ✅
- Azure infrastructure setup
- Database design (4 core tables)
- Basic authentication system
- Documentation structure
- Technology stack selection

### In Progress 🔄
- User registration flow
- Company profile management
- MudBlazor UI implementation

### Next Steps 📝
- Complete MVP features
- Implement search functionality
- Add messaging system
- Deploy to Azure App Service

## 🏗️ Architecture Overview

```
FDX.trading/
├── Components/          # Blazor components
├── Data/               # Entity models
├── Database/           # SQL scripts and migrations
├── Documentation/      # Project documentation (you are here)
├── Services/           # Business logic
├── wwwroot/           # Static assets
└── Program.cs         # Application entry point
```

## 🔑 Key Technologies

- **Backend**: .NET 9.0, Blazor Server, Entity Framework Core
- **Frontend**: MudBlazor, Razor Components
- **Database**: Azure SQL Database
- **Authentication**: Microsoft Entra ID (Azure AD)
- **Hosting**: Azure (planned)

## 📈 Project Metrics

- **Tables**: 5 (Users, Companies, UserEmployments, UserPhones, sysdiagrams)
- **Current Data**: Minimal test data (1 row per table)
- **Target Launch**: Q1 2025 (MVP)
- **Target Users**: 500+ companies by Q4 2025

## 👥 Contact & Support

For questions or support regarding this documentation:
- **Project Owner**: Udi Stryk
- **Email**: foodz-x@hotmail.com
- **Azure Admin**: 57b7b3d6-90d3-41de-90ba-a4667b260695

## 🔄 Documentation Updates

This documentation is actively maintained and updated as the project evolves. 

**Last Updated**: January 2025  
**Version**: 1.0.0

---

*FDX Trading - Revolutionizing B2B Food Exchange*
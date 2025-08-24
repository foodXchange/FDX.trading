# FDX.Trading Project Structure
**Last Updated:** August 24, 2024 at 07:40 UTC

## Solution Overview
Multi-portal food trading platform built with ASP.NET Core 8.0 and Blazor

## Main Projects

### 1. FoodX.Admin (Primary Portal - Active)
**Path:** `/FoodX.Admin/`  
**Status:** ✅ Running on http://localhost:5193  
**Purpose:** Administrative portal for managing the entire platform

**Key Components:**
- `/Components/Pages/` - Blazor pages
  - `BuyerCompanies.razor` - NEW: Buyer companies database viewer
  - `Products.razor` - Product management
  - `Companies.razor` - Company management
  - `Buyers.razor` / `Suppliers.razor` - User role management
  - Dashboard pages for each role type
- `/Components/Dialogs/` - Modal dialogs
  - `ViewBuyerCompanyDialog.razor` - NEW: Buyer company details viewer
- `/Models/` - Data models
  - `FoodXBuyer.cs` - NEW: External buyer data model
  - `Company.cs`, `Product.cs`, `User.cs` - Core entities
- `/Services/` - Business services
  - `DualModeEmailService.cs` - Email service with SendGrid
  - `MagicLinkService.cs` - Passwordless authentication
- `/Data/` - Database contexts
  - `FoodXDbContext.cs` - Main database context

### 2. FoodX.Core (Shared Library)
**Path:** `/FoodX.Core/`  
**Purpose:** Shared models, services, and repositories

**Key Components:**
- `/Models/Entities/` - Core business entities
- `/Repositories/` - Data access layer
- `/Services/` - Shared services
  - `VectorSearchService.cs` - AI-powered search
  - `SendGridEmailService.cs` - Email service

### 3. FoodX.Buyer (Portal - Planned)
**Path:** `/FoodX.Buyer/`  
**Status:** 🚧 Not yet implemented  
**Purpose:** Buyer-specific portal

### 4. FoodX.Supplier (Portal - Planned)
**Path:** `/FoodX.Supplier/`  
**Status:** 🚧 Not yet implemented  
**Purpose:** Supplier-specific portal

### 5. FoodX.Marketplace (Portal - Planned)
**Path:** `/FoodX.Marketplace/`  
**Status:** 🚧 Not yet implemented  
**Purpose:** Public marketplace portal

### 6. FoodX.SharedUI (Component Library)
**Path:** `/FoodX.SharedUI/`  
**Purpose:** Shared UI components across portals

## Database Scripts
**Path:** Root directory  
Important SQL scripts:
- `import_foodxbuyers_*.sql` - Migration scripts for buyer data
- `delete_demo_data.sql` - Cleanup test data
- `create_*.sql` - Table creation scripts
- `seed_*.sql` - Data seeding scripts

## Python Utilities
**Path:** Root directory  
- `run_sql.py` - SQL script executor
- `db_connection.py` - Database connection utility
- `import_*.py` - Data import scripts
- `test_*.py` - Testing utilities

## Documentation
**Path:** `/Documentation/`
- `SESSION_2024_08_24.md` - Today's session documentation
- `AZURE_CREDENTIALS.md` - Updated credentials and connection strings
- `PROJECT_STATUS_REPORT.md` - Overall project status
- `MULTI_PORTAL_ARCHITECTURE.md` - Architecture documentation

## Configuration Files
- `appsettings.Development.json` - Development settings (with SendGrid)
- `appsettings.Production.json` - Production settings
- `azure-pipelines.yml` - CI/CD pipeline configuration

## Current Database Tables
- **AspNetUsers** - Identity users
- **AspNetRoles** - User roles
- **Companies** - Company records (buyers/suppliers)
- **Products** - Product catalog
- **FoodXBuyers** - External buyer data (671 records)
- **FoodXSuppliers** - External supplier data
- **MagicLinkTokens** - Passwordless auth tokens
- Role-specific tables: Buyers, Suppliers, Experts, Agents

## Recent Changes (August 24, 2024)
1. Added FoodXBuyer model and integration
2. Created BuyerCompanies management page
3. Added ViewBuyerCompanyDialog component
4. Updated navigation menu with buyer companies link
5. Created migration scripts for data import
6. Cleaned up all demo/test data
7. Updated credentials documentation

## Active Services
- **SQL Server:** Azure SQL Database (fdx-sql-prod.database.windows.net)
- **Email:** SendGrid API (configured and working)
- **Authentication:** Magic Link (passwordless)
- **Application:** Running on http://localhost:5193

## Key Features Implemented
✅ User authentication (magic links)  
✅ Role-based access control  
✅ Company management  
✅ Product management  
✅ Buyer companies database viewer  
✅ Email service integration  
✅ Database connectivity  
⏳ Vector search (partially implemented)  
⏳ Multi-portal architecture (planned)

## Environment
- **OS:** Windows Server
- **Framework:** .NET 8.0
- **UI:** Blazor Server with MudBlazor
- **Database:** SQL Server 2022
- **IDE:** Visual Studio / VS Code
- **Source Control:** Git
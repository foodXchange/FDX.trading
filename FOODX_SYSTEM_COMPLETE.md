# FoodX Trading Platform - Complete System Documentation

## 🚀 System Overview
**Date**: December 9, 2024  
**Version**: 2.0  
**Status**: Production Ready with Development Bypass

## 📋 Table of Contents
1. [Architecture](#architecture)
2. [Core Components](#core-components)
3. [Email System](#email-system)
4. [Authentication & Security](#authentication--security)
5. [Database Schema](#database-schema)
6. [API Endpoints](#api-endpoints)
7. [Development Setup](#development-setup)
8. [Deployment](#deployment)

---

## 🏗️ Architecture

### Microservices
```
┌─────────────────────────────────────────────────────────┐
│                   FoodX Trading Platform                 │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌──────────────┐     ┌──────────────┐                 │
│  │ FoodX.Admin  │────▶│ Email Service│                 │
│  │  Port: 5195  │     │  Port: 5257  │                 │
│  └──────────────┘     └──────────────┘                 │
│         │                     │                         │
│         ▼                     ▼                         │
│  ┌──────────────────────────────────┐                  │
│  │     Azure SQL Database           │                  │
│  │  - fdxdb (Main)                  │                  │
│  │  - FoodXEmails (Email)           │                  │
│  └──────────────────────────────────┘                  │
│                                                          │
│  ┌──────────────────────────────────┐                  │
│  │     External Services            │                  │
│  │  - SendGrid (Email)              │                  │
│  │  - Azure OpenAI (AI)             │                  │
│  │  - Azure Key Vault (Secrets)     │                  │
│  └──────────────────────────────────┘                  │
└─────────────────────────────────────────────────────────┘
```

---

## 🔧 Core Components

### 1. **FoodX.Admin** (Main Application)
- **Technology**: ASP.NET Core 9.0, Blazor Server, MudBlazor
- **Port**: 5195
- **Features**:
  - Supplier Management
  - Product Catalog (500K+ products)
  - RFQ System
  - AI-Powered Search
  - Email Integration
  - Portal System (Buyer/Supplier)

### 2. **FoodX.EmailService** (Microservice)
- **Technology**: ASP.NET Core Web API
- **Port**: 5257
- **Features**:
  - SendGrid Integration
  - Inbound Email Processing
  - Thread Management
  - Webhook Handling
  - Email Storage

### 3. **FoodX.Core** (Shared Library)
- Vector Search Services
- Embedding Services
- Caching Services
- Common Models

### 4. **FoodX.SharedUI** (UI Components)
- Shared MudBlazor Components
- Common Layouts
- Reusable Dialogs

---

## 📧 Email System

### Features Implemented
✅ **Send Emails** via SendGrid API  
✅ **Receive Emails** via SendGrid Webhooks  
✅ **Email Inbox** with folder navigation  
✅ **Email Compose** with HTML support  
✅ **Thread Management** for conversations  
✅ **Category System** (RFQ, Quote, Order, Support)  
✅ **Development Bypass** for testing  

### Email Service Endpoints
```http
POST   /api/email/send
GET    /api/email/inbox
GET    /api/email/{id}
DELETE /api/email/{id}
POST   /api/email/{id}/mark-read
POST   /api/email/{id}/resend
GET    /api/email/thread/{threadId}
POST   /api/webhook/sendgrid/inbound
```

### SendGrid Configuration
- **API Key**: Stored in Azure Key Vault
- **Inbound Domain**: email.fdx.trading
- **MX Records**: Configured for mx.sendgrid.net
- **Webhook URL**: https://email.fdx.trading/api/webhook/sendgrid/inbound

---

## 🔐 Authentication & Security

### Development Bypass (Active)
```csharp
// Auto-login enabled for development
// Any email can be used to login
// Located in: MagicLinkLogin.razor & MagicLinkRequest.razor
```

### Production Authentication
- **Magic Link System** (passwordless)
- **Role-Based Access**: SuperAdmin, Admin, Buyer, Supplier, Agent, Expert
- **Azure Key Vault** for secrets
- **JWT Tokens** for API authentication

### Special Users
- **udi@fdx.trading** - SuperAdmin role

---

## 🗄️ Database Schema

### Main Database (fdxdb)
```sql
-- Core Tables
AspNetUsers (Identity)
AspNetRoles
FoodXSuppliers (2000+ suppliers)
Products (500K+ products)
RFQs
Quotes
Orders

-- Support Tables
EmailThreads
Emails
EmailAttachments
```

### Key Relationships
```
Supplier ──1:N──> Products
Supplier ──1:N──> RFQs
RFQ ──1:N──> Quotes
User ──1:N──> Emails
Email ──N:1──> EmailThread
```

---

## 🔌 API Endpoints

### Test Endpoints
```http
GET  /api/test/health
GET  /api/test/users-by-role
POST /api/test/reset-passwords
POST /api/test/create-superadmin
```

### Business Endpoints
```http
GET  /api/suppliers
GET  /api/products/search?q={query}
POST /api/rfq/create
POST /api/quote/submit
GET  /api/portal/dashboard
```

---

## 🛠️ Development Setup

### Prerequisites
- .NET 9.0 SDK
- SQL Server or Azure SQL
- SendGrid Account
- Azure Subscription (Key Vault, OpenAI)

### Local Development
```bash
# Clone repository
git clone https://github.com/fdxtrading/FDX.trading.git

# Run Email Service
cd FoodX.EmailService
dotnet run

# Run Main Application
cd FoodX.Admin
dotnet run

# Access
http://localhost:5195 - Main App
http://localhost:5257 - Email Service
```

### Environment Variables
```env
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=<SQL_CONNECTION>
AzureKeyVault__VaultUri=https://fdx-keyvault-prod.vault.azure.net/
SendGrid__ApiKey=<SENDGRID_KEY>
EmailService__BaseUrl=http://localhost:5257
```

---

## 🚀 Deployment

### Azure Resources
- **App Service**: fdx-webapp-prod
- **SQL Server**: fdx-sql-prod
- **Key Vault**: fdx-keyvault-prod
- **Storage**: fdxstorageprod

### Deployment Commands
```powershell
# Build and publish
dotnet publish -c Release -o ./publish

# Deploy to Azure
az webapp deploy --resource-group fdx-rg --name fdx-webapp-prod --src-path ./publish.zip
```

### ngrok Setup (for webhooks)
```bash
# Installed and configured
ngrok authtoken 32QXbwJKrOdannc3iqAXwYypX4O_88FtXQn1PjFsNi5gHN1Hm
ngrok http 5257
```

---

## 📊 Current Statistics
- **Suppliers**: 2,000+
- **Products**: 500,000+
- **Countries**: 100+
- **Categories**: 50+
- **Active Users**: Development Mode

---

## 🔄 Recent Updates
1. ✅ Email Service microservice implementation
2. ✅ SendGrid integration (send & receive)
3. ✅ Development authentication bypass
4. ✅ Email UI components (Inbox, Compose)
5. ✅ SuperAdmin role for udi@fdx.trading
6. ✅ Fixed navigation and DI issues

---

## 📝 Next Steps
1. 🔲 Rich text editor for emails
2. 🔲 Email templates system
3. 🔲 File attachments
4. 🔲 Real-time notifications
5. 🔲 Email analytics dashboard

---

## 🐛 Known Issues
- None currently

## 📞 Support
- **Admin**: udi@fdx.trading
- **GitHub**: https://github.com/fdxtrading/FDX.trading

---

*Generated with Claude Code 🤖*  
*Co-Authored-By: Claude <noreply@anthropic.com>*
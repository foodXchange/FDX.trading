# FoodX Trading Platform - Technical Documentation

## Table of Contents
1. [Technology Stack](#technology-stack)
2. [Architecture Overview](#architecture-overview)
3. [Database Architecture](#database-architecture)
4. [Database Schema](#database-schema)
5. [Performance Optimizations](#performance-optimizations)
6. [Security Features](#security-features)
7. [Development Guidelines](#development-guidelines)

## Technology Stack

### Frontend
- **Framework**: Blazor Server (.NET 9)
- **UI Library**: MudBlazor 7.22.0
- **Rendering**: Server-side rendering with SignalR
- **Authentication**: ASP.NET Core Identity

### Backend
- **Runtime**: .NET 9.0
- **Language**: C# 13
- **API**: RESTful services with built-in controllers
- **ORM**: Entity Framework Core 9.0

### Database
- **Primary**: Azure SQL Database (Production)
- **Development**: SQL Server LocalDB
- **Connection**: Azure Active Directory Authentication
- **Resilience**: Built-in retry logic and connection pooling

### Cloud Services
- **Hosting**: Azure App Service
- **Database**: Azure SQL Database
- **Key Vault**: Azure Key Vault for secrets management
- **Email**: SendGrid for transactional emails
- **Authentication**: Azure AD Integration

### DevOps
- **Version Control**: Git
- **CI/CD**: Azure DevOps / GitHub Actions
- **Monitoring**: Application Insights
- **Logging**: ILogger with Console, Debug, and EventLog providers

## Architecture Overview

### Layered Architecture

```
┌─────────────────────────────────────────┐
│         Presentation Layer               │
│  (Blazor Components, Razor Pages)        │
├─────────────────────────────────────────┤
│         Service Layer                    │
│  (Business Logic, Email, Auth)           │
├─────────────────────────────────────────┤
│         Repository Layer                 │
│  (Unit of Work, Generic Repository)      │
├─────────────────────────────────────────┤
│         Data Access Layer                │
│  (Entity Framework Core, DbContext)      │
├─────────────────────────────────────────┤
│         Database Layer                   │
│  (Azure SQL Database)                    │
└─────────────────────────────────────────┘
```

### Design Patterns Implemented

1. **Repository Pattern**: Generic repository with Unit of Work
2. **Dependency Injection**: Constructor injection throughout
3. **Factory Pattern**: Logger factory for creating typed loggers
4. **Interceptor Pattern**: Database performance monitoring
5. **Cache-Aside Pattern**: Memory caching for frequently accessed data

### Project Structure

```
FoodX.Admin/
├── Components/              # Blazor components
│   ├── Account/            # Authentication pages
│   ├── Layout/             # Layout components
│   └── Pages/              # Page components
├── Data/                   # Data layer
│   ├── ApplicationDbContext.cs
│   ├── FoodXDbContext.cs
│   ├── DatabaseConfiguration.cs
│   └── PerformanceInterceptor.cs
├── Models/                 # Domain models
│   ├── Company.cs
│   ├── Product.cs
│   ├── Buyer.cs
│   └── Supplier.cs
├── Repositories/           # Repository pattern
│   ├── IRepository.cs
│   ├── Repository.cs
│   ├── IUnitOfWork.cs
│   └── UnitOfWork.cs
├── Services/               # Business services
│   ├── MagicLinkService.cs
│   ├── SendGridEmailService.cs
│   ├── MemoryCacheService.cs
│   └── RoleNavigationService.cs
└── Program.cs              # Application configuration
```

## Database Architecture

### Connection Configuration

```csharp
// Optimized connection string with pooling
MinPoolSize: 5
MaxPoolSize: 100
Connection Timeout: 30
Connect Retry Count: 3
Connect Retry Interval: 10
Multiple Active Result Sets: true
```

### Resilience Features

1. **Automatic Retry Logic**
   - Max retry count: 5
   - Max retry delay: 30 seconds
   - Handles transient failures automatically

2. **Connection Pooling**
   - Minimum pool size: 5 connections
   - Maximum pool size: 100 connections
   - Efficient connection reuse

3. **Command Timeout**
   - Default: 60 seconds
   - Configurable per query

4. **Query Splitting**
   - Automatic query splitting for better performance
   - Reduces cartesian explosion with multiple includes

### Performance Monitoring

- **Performance Interceptor**: Logs slow queries (>1000ms)
- **Query Tracking**: NoTrackingWithIdentityResolution by default
- **Caching**: In-memory caching with sliding expiration
- **Async/Await**: All database operations are async

## Database Schema

### Identity Tables (ApplicationDbContext)

```sql
-- Core Identity Tables
AspNetUsers
├── Id (nvarchar(450), PK)
├── UserName (nvarchar(256))
├── Email (nvarchar(256))
├── EmailConfirmed (bit)
├── PasswordHash (nvarchar(max))
├── PhoneNumber (nvarchar(max))
├── FirstName (nvarchar(100))
├── LastName (nvarchar(100))
└── [Additional Identity columns]

AspNetRoles
├── Id (nvarchar(450), PK)
├── Name (nvarchar(256))
└── NormalizedName (nvarchar(256))

AspNetUserRoles
├── UserId (nvarchar(450), FK)
└── RoleId (nvarchar(450), FK)

MagicLinkTokens
├── Id (int, PK)
├── Token (nvarchar(max))
├── UserId (nvarchar(450), FK)
├── Email (nvarchar(max))
├── CreatedAt (datetime2)
├── ExpiresAt (datetime2)
├── IsUsed (bit)
└── UsedAt (datetime2, nullable)
```

### Business Tables (FoodXDbContext)

```sql
Companies
├── Id (int, PK, Identity)
├── Name (nvarchar(200))
├── TaxId (nvarchar(50))
├── Address (nvarchar(500))
├── City (nvarchar(100))
├── Country (nvarchar(100))
├── Phone (nvarchar(50))
├── Email (nvarchar(100))
├── Website (nvarchar(200))
├── CreatedAt (datetime2)
└── UpdatedAt (datetime2)

Products
├── Id (int, PK, Identity)
├── Name (nvarchar(200))
├── Category (nvarchar(100))
├── Description (nvarchar(max))
├── Unit (nvarchar(50))
├── MinOrderQuantity (decimal)
├── Price (decimal)
├── SupplierId (int, FK)
├── CreatedAt (datetime2)
└── UpdatedAt (datetime2)

Buyers
├── Id (int, PK, Identity)
├── UserId (nvarchar(450), FK)
├── CompanyId (int, FK)
├── Department (nvarchar(100))
├── PurchaseLimit (decimal)
├── IsActive (bit)
├── CreatedAt (datetime2)
└── UpdatedAt (datetime2)

Suppliers
├── Id (int, PK, Identity)
├── UserId (nvarchar(450), FK)
├── CompanyId (int, FK)
├── ContactPerson (nvarchar(200))
├── PaymentTerms (nvarchar(100))
├── DeliveryTerms (nvarchar(100))
├── Rating (decimal)
├── IsVerified (bit)
├── CreatedAt (datetime2)
└── UpdatedAt (datetime2)

Invitations
├── Id (int, PK, Identity)
├── Code (nvarchar(50), Unique)
├── Email (nvarchar(256))
├── Role (nvarchar(50))
├── CompanyId (int, FK, nullable)
├── InvitedBy (nvarchar(450), FK)
├── CreatedAt (datetime2)
├── ExpiresAt (datetime2)
├── IsUsed (bit)
└── UsedAt (datetime2, nullable)
```

## Performance Optimizations

### 1. Database Level
- **Connection Pooling**: Min 5, Max 100 connections
- **Query Splitting**: Automatic for complex queries
- **No Tracking**: Default for read operations
- **Indexed Columns**: Email, UserId, CompanyId
- **Retry Logic**: Automatic retry for transient failures

### 2. Application Level
- **Memory Caching**: In-memory cache with sliding expiration
- **Async Operations**: All I/O operations are async
- **Lazy Loading**: Disabled to prevent N+1 queries
- **Batch Operations**: Unit of Work pattern for transactions
- **Circuit Optimization**: Blazor circuit settings optimized

### 3. Query Optimization
- **Performance Interceptor**: Monitors and logs slow queries
- **Command Timeout**: 60 seconds default
- **Connection Resilience**: Automatic retry with exponential backoff
- **Query Filters**: Global query filters for soft deletes

## Security Features

### Authentication & Authorization
- **ASP.NET Core Identity**: Full-featured authentication
- **Role-Based Access**: Admin, Buyer, Seller, Agent roles
- **Magic Link Authentication**: Passwordless login option
- **Two-Factor Authentication**: Support for 2FA
- **Account Lockout**: After 5 failed attempts

### Data Protection
- **Azure Key Vault**: Secure storage of secrets
- **Connection Encryption**: TLS/SSL for all connections
- **Password Requirements**: 
  - Minimum 8 characters
  - Uppercase, lowercase, digit, special character
- **HTTPS Only**: Enforced in production
- **HSTS**: HTTP Strict Transport Security enabled

### Security Headers
- **Content Security Policy**: Restricts resource loading
- **X-Frame-Options**: Prevents clickjacking
- **X-Content-Type-Options**: Prevents MIME sniffing
- **Referrer Policy**: Controls referrer information

## Development Guidelines

### Code Standards
1. **Naming Conventions**
   - PascalCase for public members
   - camelCase for private fields
   - Async methods end with "Async"

2. **Async/Await Best Practices**
   - Always use async for I/O operations
   - ConfigureAwait(false) in library code
   - Avoid async void except for event handlers

3. **Dependency Injection**
   - Constructor injection preferred
   - Scoped lifetime for DbContext
   - Singleton for caching services

4. **Error Handling**
   - Global exception handler
   - Structured logging with ILogger
   - User-friendly error messages

### Testing Guidelines
1. **Unit Tests**: Repository and service layer
2. **Integration Tests**: API endpoints
3. **Performance Tests**: Database queries
4. **Security Tests**: Authentication flows

### Deployment Checklist
- [ ] Update connection strings
- [ ] Configure Azure Key Vault
- [ ] Set environment to Production
- [ ] Enable HTTPS redirection
- [ ] Configure logging levels
- [ ] Set up monitoring
- [ ] Database migrations applied
- [ ] SendGrid API key configured

## Monitoring & Logging

### Logging Providers
- **Console**: Development environment
- **Debug**: Development environment
- **EventLog**: Production (Windows only)
- **Application Insights**: Production monitoring

### Performance Metrics
- Query execution time
- Circuit connection status
- Memory usage
- Response time
- Error rates

### Health Checks
- Database connectivity
- External service availability
- Memory usage
- CPU usage

## Maintenance Tasks

### Regular Tasks
1. **Database Maintenance**
   - Index rebuilding
   - Statistics updates
   - Transaction log backup

2. **Performance Review**
   - Analyze slow query logs
   - Review memory usage
   - Check connection pool metrics

3. **Security Updates**
   - Update NuGet packages
   - Review security advisories
   - Rotate API keys

### Troubleshooting Guide
1. **Connection Issues**
   - Check connection string
   - Verify firewall rules
   - Review Azure SQL settings

2. **Performance Issues**
   - Check slow query logs
   - Review memory cache hit rate
   - Analyze connection pool usage

3. **Authentication Issues**
   - Verify Azure AD configuration
   - Check token expiration
   - Review role assignments

## Contact & Support

For technical support or questions about this documentation:
- Email: udi@fdx.trading
- Platform: FoodX B2B Trading Platform

---

*Last Updated: January 2025*
*Version: 1.0.0*
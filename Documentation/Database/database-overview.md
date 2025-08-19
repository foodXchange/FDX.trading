# FDX Trading Database Overview

## Database Information
- **Platform**: Azure SQL Database
- **Server**: fdx-sql-prod.database.windows.net
- **Database Name**: fdxdb
- **Schema**: dbo (default)
- **Collation**: SQL_Latin1_General_CP1_CI_AS (default)
- **Service Tier**: (To be confirmed)
- **Region**: (To be confirmed)

## Database Purpose
The FDX Trading database supports a B2B food exchange platform connecting buyers and suppliers in the food industry. It manages:
- User accounts and authentication
- Company profiles (buyers and suppliers)
- User-company relationships (employment)
- Contact information

## Current Database Statistics
- **Total Tables**: 5 (4 application tables + 1 system table)
- **Total Rows**: ~4 (minimal test data)
- **Primary Schema**: dbo

## Tables Overview

### Core Business Tables
1. **Users** - User account management
2. **Companies** - Business entity profiles
3. **UserEmployments** - User-company associations
4. **UserPhones** - User contact numbers

### System Tables
5. **sysdiagrams** - SQL Server diagram storage (empty)

## Database Design Patterns

### Relationships
- **Many-to-Many**: Users ↔ Companies (through UserEmployments)
- **One-to-Many**: Users → UserPhones

### Naming Conventions
- **Tables**: PascalCase plural (Users, Companies)
- **Columns**: PascalCase
- **Primary Keys**: Id (integer, identity)
- **Foreign Keys**: TableNameId (e.g., UserId, CompanyId)

### Common Column Patterns
- **Audit Fields**: CreatedAt, UpdatedAt (datetime2)
- **Status Fields**: IsActive (bit)
- **Identity**: Id (int, IDENTITY)

## Security Features
- Encrypted connections required
- Microsoft Entra ID authentication supported
- SQL authentication available
- Row-level security ready (not yet implemented)

## Entity Framework Integration
- Code-First approach with migrations
- DbContext: FdxDbContext
- Models in: Data\Entities.cs
- Migration scripts in: Database\Schema\

## Backup and Recovery
- Azure automatic backups enabled
- Point-in-time restore available (last 7-35 days)
- Geo-redundant backups (if configured)

## Performance Considerations
- Currently minimal data (development phase)
- Indexes on primary keys and foreign keys
- Unique index on Users.Email
- Computed column: Users.FullName

## Future Considerations
- Add more business entities (Products, Orders, Transactions)
- Implement audit logging tables
- Add stored procedures for complex operations
- Consider partitioning for large tables
- Implement row-level security for multi-tenant access
# Database Schema Documentation

## Users Table
Primary table for user account management and authentication.

### Structure
```sql
CREATE TABLE [dbo].[Users] (
    Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Title        NVARCHAR(10) NULL,
    FirstName    NVARCHAR(100) NOT NULL,
    LastName     NVARCHAR(100) NOT NULL,
    Email        NVARCHAR(256) NOT NULL,
    PasswordHash NVARCHAR(500) NULL,
    Role         NVARCHAR(50) NOT NULL,
    IsActive     BIT NOT NULL,
    CreatedAt    DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt    DATETIME2(7) NOT NULL,
    FullName AS  (LTRIM(RTRIM(ISNULL(Title + ' ', '') + FirstName + ' ' + LastName)))
)
```

### Key Features
- **Primary Key**: Id
- **Unique Index**: Email (UQ_Users_Email)
- **Check Constraint**: Role IN ('Buyer', 'Seller', 'Agent', 'Admin')
- **Computed Column**: FullName (concatenation of Title, FirstName, LastName)

### Column Details
| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| Id | int | NO | IDENTITY | Primary key |
| Title | nvarchar(10) | YES | NULL | Mr., Ms., Dr., etc. |
| FirstName | nvarchar(100) | NO | - | User's first name |
| LastName | nvarchar(100) | NO | - | User's last name |
| Email | nvarchar(256) | NO | - | Unique email address |
| PasswordHash | nvarchar(500) | YES | NULL | Hashed password |
| Role | nvarchar(50) | NO | - | User role (Buyer/Seller/Agent/Admin) |
| IsActive | bit | NO | - | Account active status |
| CreatedAt | datetime2 | NO | SYSUTCDATETIME() | Account creation timestamp |
| UpdatedAt | datetime2 | NO | - | Last update timestamp |
| FullName | nvarchar(212) | - | Computed | Full name display |

---

## Companies Table
Stores business entity information for both buyers and suppliers.

### Structure
```sql
CREATE TABLE [dbo].[Companies] (
    Id               INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name             NVARCHAR(200) NOT NULL,
    CompanyType      NVARCHAR(30) NOT NULL DEFAULT 'Buyer',
    BuyerCategory    NVARCHAR(50) NULL,
    Country          NVARCHAR(100) NULL,
    Website          NVARCHAR(200) NULL,
    MainEmail        NVARCHAR(256) NULL,
    MainPhone        NVARCHAR(50) NULL,
    VatNumber        NVARCHAR(50) NULL,
    Address          NVARCHAR(300) NULL,
    WarehouseAddress NVARCHAR(300) NULL,
    Description      NVARCHAR(1000) NULL,
    LogoUrl          NVARCHAR(500) NULL,
    IsActive         BIT NOT NULL DEFAULT 1,
    CreatedAt        DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt        DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
)
```

### Key Features
- **Primary Key**: Id
- **Check Constraint**: CompanyType IN ('Buyer', 'Supplier')
- **Default Values**: CompanyType='Buyer', IsActive=1

### Column Details
| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| Id | int | NO | IDENTITY | Primary key |
| Name | nvarchar(200) | NO | - | Company name |
| CompanyType | nvarchar(30) | NO | 'Buyer' | Buyer or Supplier |
| BuyerCategory | nvarchar(50) | YES | NULL | Category for buyers |
| Country | nvarchar(100) | YES | NULL | Company country |
| Website | nvarchar(200) | YES | NULL | Company website URL |
| MainEmail | nvarchar(256) | YES | NULL | Primary contact email |
| MainPhone | nvarchar(50) | YES | NULL | Primary phone number |
| VatNumber | nvarchar(50) | YES | NULL | VAT/Tax number |
| Address | nvarchar(300) | YES | NULL | Business address |
| WarehouseAddress | nvarchar(300) | YES | NULL | Warehouse location |
| Description | nvarchar(1000) | YES | NULL | Company description |
| LogoUrl | nvarchar(500) | YES | NULL | Logo image URL |
| IsActive | bit | NO | 1 | Active status |
| CreatedAt | datetime2 | NO | SYSUTCDATETIME() | Creation timestamp |
| UpdatedAt | datetime2 | NO | SYSUTCDATETIME() | Last update timestamp |

---

## UserEmployments Table
Junction table linking users to companies with employment details.

### Structure
```sql
CREATE TABLE [dbo].[UserEmployments] (
    Id        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId    INT NOT NULL,
    CompanyId INT NOT NULL,
    JobTitle  NVARCHAR(100) NULL,
    IsPrimary BIT NOT NULL DEFAULT 1,
    StartDate DATE NULL,
    EndDate   DATE NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (CompanyId) REFERENCES Companies(Id)
)
```

### Key Features
- **Primary Key**: Id
- **Foreign Keys**: UserId → Users.Id, CompanyId → Companies.Id
- **Unique Constraint**: (UserId, CompanyId) - One employment per user-company pair

### Column Details
| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| Id | int | NO | IDENTITY | Primary key |
| UserId | int | NO | - | Reference to Users.Id |
| CompanyId | int | NO | - | Reference to Companies.Id |
| JobTitle | nvarchar(100) | YES | NULL | Position/role in company |
| IsPrimary | bit | NO | 1 | Primary employment flag |
| StartDate | date | YES | NULL | Employment start date |
| EndDate | date | YES | NULL | Employment end date |
| CreatedAt | datetime2 | NO | SYSUTCDATETIME() | Record creation timestamp |

---

## UserPhones Table
Stores multiple phone numbers per user with type classification.

### Structure
```sql
CREATE TABLE [dbo].[UserPhones] (
    Id          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId      INT NOT NULL,
    PhoneType   NVARCHAR(50) NOT NULL,
    PhoneNumber NVARCHAR(50) NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
)
```

### Key Features
- **Primary Key**: Id
- **Foreign Key**: UserId → Users.Id
- **Phone Types**: Mobile, Work, Home, Fax, Other

### Column Details
| Column | Type | Nullable | Default | Description |
|--------|------|----------|---------|-------------|
| Id | int | NO | IDENTITY | Primary key |
| UserId | int | NO | - | Reference to Users.Id |
| PhoneType | nvarchar(50) | NO | - | Type of phone (Mobile/Work/Home) |
| PhoneNumber | nvarchar(50) | NO | - | Phone number |

---

## Database Relationships Diagram

```
┌─────────────┐         ┌──────────────────┐         ┌─────────────┐
│   Users     │────┐    │ UserEmployments  │    ┌────│  Companies  │
├─────────────┤    │    ├──────────────────┤    │    ├─────────────┤
│ Id (PK)     │    └───→│ UserId (FK)      │    │    │ Id (PK)     │
│ Email       │         │ CompanyId (FK)   │←───┘    │ Name        │
│ FirstName   │         │ JobTitle         │         │ CompanyType │
│ LastName    │         │ IsPrimary        │         │ Country     │
│ Role        │         └──────────────────┘         │ IsActive    │
│ IsActive    │                                      └─────────────┘
└─────────────┘         
       │                
       │                ┌──────────────────┐
       └───────────────→│   UserPhones     │
                        ├──────────────────┤
                        │ Id (PK)          │
                        │ UserId (FK)      │
                        │ PhoneType        │
                        │ PhoneNumber      │
                        └──────────────────┘
```

## Indexes

### Users Table
- **Primary Key**: PK_Users on Id
- **Unique Index**: UQ_Users_Email on Email

### Companies Table
- **Primary Key**: PK_Companies on Id
- **Index**: IX_Companies_IsActive on IsActive (suggested)
- **Index**: IX_Companies_CompanyType on CompanyType (suggested)

### UserEmployments Table
- **Primary Key**: PK_UserEmployments on Id
- **Foreign Key Index**: IX_UserEmployments_UserId
- **Foreign Key Index**: IX_UserEmployments_CompanyId

### UserPhones Table
- **Primary Key**: PK_UserPhones on Id
- **Foreign Key Index**: IX_UserPhones_UserId
-- Drop existing tables if they exist
IF EXISTS (SELECT * FROM sysobjects WHERE name='OrderItems' AND xtype='U')
    DROP TABLE OrderItems;
IF EXISTS (SELECT * FROM sysobjects WHERE name='Orders' AND xtype='U')
    DROP TABLE Orders;
IF EXISTS (SELECT * FROM sysobjects WHERE name='Exhibitors' AND xtype='U')
    DROP TABLE Exhibitors;
IF EXISTS (SELECT * FROM sysobjects WHERE name='Exhibitions' AND xtype='U')
    DROP TABLE Exhibitions;
IF EXISTS (SELECT * FROM sysobjects WHERE name='FoodXBuyers' AND xtype='U')
    DROP TABLE FoodXBuyers;
IF EXISTS (SELECT * FROM sysobjects WHERE name='FoodXSuppliers' AND xtype='U')
    DROP TABLE FoodXSuppliers;
IF EXISTS (SELECT * FROM sysobjects WHERE name='FoodXProducts' AND xtype='U')
    DROP TABLE FoodXProducts;

-- Create FoodXBuyers table (matching Buyer entity model)
CREATE TABLE FoodXBuyers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Company NVARCHAR(200) NOT NULL,
    Type NVARCHAR(100),
    Website NVARCHAR(500),
    Categories NVARCHAR(200),
    Size NVARCHAR(50),
    Stores NVARCHAR(100),
    Region NVARCHAR(100),
    Markets NVARCHAR(200),
    Domain NVARCHAR(200),
    ProcurementEmail NVARCHAR(200),
    ProcurementPhone NVARCHAR(50),
    ProcurementManager NVARCHAR(200),
    CommercialManager NVARCHAR(200),
    GeneralEmail NVARCHAR(200),
    GeneralPhone NVARCHAR(50),
    CertificationsRequired NVARCHAR(MAX),
    PaymentTerms NVARCHAR(200),
    MinimumOrderValue DECIMAL(18,2),
    AnnualPurchaseVolume DECIMAL(18,2),
    PreferredDeliveryTerms NVARCHAR(200),
    QualityStandards NVARCHAR(MAX),
    ComplianceRequirements NVARCHAR(MAX),
    Notes NVARCHAR(MAX),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
    CreatedBy NVARCHAR(450),
    UpdatedBy NVARCHAR(450)
);

-- Create FoodXSuppliers table (matching Supplier entity model)
CREATE TABLE FoodXSuppliers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SupplierName NVARCHAR(200) NOT NULL,
    CompanyWebsite NVARCHAR(500),
    Description NVARCHAR(MAX),
    ProductCategory NVARCHAR(200),
    Address NVARCHAR(500),
    CompanyEmail NVARCHAR(200),
    Phone NVARCHAR(50),
    Products NVARCHAR(MAX),
    Country NVARCHAR(100),
    PaymentTerms NVARCHAR(200),
    Incoterms NVARCHAR(100),
    MinimumOrderQuantity NVARCHAR(100),
    LeadTime NVARCHAR(100),
    Certifications NVARCHAR(MAX),
    QualityControl NVARCHAR(MAX),
    PackagingOptions NVARCHAR(MAX),
    ShippingMethods NVARCHAR(MAX),
    ProductionCapacity NVARCHAR(200),
    EstablishedYear INT,
    NumberOfEmployees INT,
    AnnualRevenue DECIMAL(18,2),
    ExportPercentage DECIMAL(5,2),
    MainMarkets NVARCHAR(MAX),
    ContactPerson NVARCHAR(200),
    ContactPosition NVARCHAR(100),
    ContactEmail NVARCHAR(200),
    ContactPhone NVARCHAR(50),
    Notes NVARCHAR(MAX),
    IsActive BIT DEFAULT 1,
    IsVerified BIT DEFAULT 0,
    VerifiedDate DATETIME,
    Rating DECIMAL(3,2),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
    CreatedBy NVARCHAR(450),
    UpdatedBy NVARCHAR(450)
);

-- Create Exhibitions table
CREATE TABLE Exhibitions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    SourceUrl NVARCHAR(500),
    Location NVARCHAR(200),
    StartDate DATETIME,
    EndDate DATETIME,
    Description NVARCHAR(MAX),
    Organizer NVARCHAR(200),
    WebsiteUrl NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME
);

-- Create Exhibitors table
CREATE TABLE Exhibitors (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CompanyName NVARCHAR(200) NOT NULL,
    ProfileUrl NVARCHAR(500),
    Country NVARCHAR(100),
    Products NVARCHAR(MAX),
    Contact NVARCHAR(500),
    Email NVARCHAR(200),
    Phone NVARCHAR(50),
    Website NVARCHAR(500),
    StandNumber NVARCHAR(50),
    Categories NVARCHAR(MAX),
    Description NVARCHAR(MAX),
    ExhibitionId INT,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
    FOREIGN KEY (ExhibitionId) REFERENCES Exhibitions(Id)
);

-- Create FoodXProducts table
CREATE TABLE FoodXProducts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    SKU NVARCHAR(100),
    Category NVARCHAR(100),
    SubCategory NVARCHAR(100),
    Description NVARCHAR(MAX),
    Unit NVARCHAR(50),
    PackSize NVARCHAR(100),
    MinOrderQuantity DECIMAL(18,2),
    Price DECIMAL(18,2),
    Currency NVARCHAR(3) DEFAULT 'USD',
    SupplierId INT,
    Brand NVARCHAR(100),
    Origin NVARCHAR(100),
    ShelfLife NVARCHAR(100),
    StorageConditions NVARCHAR(200),
    Certifications NVARCHAR(MAX),
    Ingredients NVARCHAR(MAX),
    NutritionalInfo NVARCHAR(MAX),
    Allergens NVARCHAR(MAX),
    ImageUrl NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    InStock BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
    FOREIGN KEY (SupplierId) REFERENCES FoodXSuppliers(Id)
);

-- Create Orders table
CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderNumber NVARCHAR(50) NOT NULL,
    OrderDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(50) DEFAULT 'Pending',
    BuyerId INT,
    SupplierId INT,
    TotalAmount DECIMAL(18,2),
    TaxAmount DECIMAL(18,2),
    ShippingCost DECIMAL(18,2),
    DiscountAmount DECIMAL(18,2),
    Currency NVARCHAR(3) DEFAULT 'USD',
    PaymentTerms NVARCHAR(100),
    PaymentStatus NVARCHAR(100),
    PaymentDueDate DATETIME,
    DeliveryTerms NVARCHAR(100),
    ExpectedDeliveryDate DATETIME,
    ActualDeliveryDate DATETIME,
    ShippingAddress NVARCHAR(MAX),
    BillingAddress NVARCHAR(MAX),
    PONumber NVARCHAR(100),
    Notes NVARCHAR(MAX),
    InternalNotes NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
    CreatedBy NVARCHAR(450),
    UpdatedBy NVARCHAR(450),
    FOREIGN KEY (BuyerId) REFERENCES FoodXBuyers(Id),
    FOREIGN KEY (SupplierId) REFERENCES FoodXSuppliers(Id)
);

-- Create OrderItems table
CREATE TABLE OrderItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    ProductName NVARCHAR(200),
    SKU NVARCHAR(100),
    Quantity DECIMAL(18,2) NOT NULL,
    Unit NVARCHAR(50),
    UnitPrice DECIMAL(18,2) NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    Discount DECIMAL(18,2),
    Tax DECIMAL(18,2),
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    FOREIGN KEY (ProductId) REFERENCES FoodXProducts(Id)
);

-- Create indexes for better performance
CREATE INDEX IX_FoodXBuyers_Company ON FoodXBuyers(Company);
CREATE INDEX IX_FoodXBuyers_Region ON FoodXBuyers(Region);
CREATE INDEX IX_FoodXSuppliers_SupplierName ON FoodXSuppliers(SupplierName);
CREATE INDEX IX_FoodXSuppliers_Country ON FoodXSuppliers(Country);
CREATE INDEX IX_Exhibitors_CompanyName ON Exhibitors(CompanyName);
CREATE INDEX IX_Exhibitors_ExhibitionId ON Exhibitors(ExhibitionId);
CREATE INDEX IX_FoodXProducts_Name ON FoodXProducts(Name);
CREATE INDEX IX_FoodXProducts_SupplierId ON FoodXProducts(SupplierId);
CREATE INDEX IX_Orders_BuyerId ON Orders(BuyerId);
CREATE INDEX IX_Orders_SupplierId ON Orders(SupplierId);
CREATE INDEX IX_Orders_OrderNumber ON Orders(OrderNumber);

PRINT 'All FoodX tables created successfully!';
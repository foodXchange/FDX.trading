-- =====================================================
-- ADD ENHANCED COLUMNS TO FoodXSuppliers TABLE
-- =====================================================

PRINT 'Adding enhanced columns to FoodXSuppliers table...';
PRINT '';

-- Add new columns for enhanced data
ALTER TABLE FoodXSuppliers ADD
    -- Product and sourcing information
    ProductsList NVARCHAR(MAX),
    BrandsList NVARCHAR(MAX),
    Categories NVARCHAR(MAX),
    KosherCertification BIT,
    OtherCertifications NVARCHAR(MAX),
    
    -- Contact and location
    PrimaryEmail NVARCHAR(200),
    AllEmails NVARCHAR(MAX),
    PrimaryPhone NVARCHAR(50),
    ClosestSeaPort NVARCHAR(200),
    DistanceToSeaPort INT,
    
    -- Business information
    SupplierCode NVARCHAR(100),
    SupplierVATNumber NVARCHAR(100),
    YearFounded INT,
    SupplierType NVARCHAR(100),
    
    -- Sourcing and logistics
    PickingAddress NVARCHAR(500),
    IncotermsPrice NVARCHAR(100),
    SourcingStages NVARCHAR(200),
    
    -- Data quality
    ExtractionStatus NVARCHAR(50),
    ProductsFound INT,
    BrandsFound INT,
    DataSource NVARCHAR(200),
    FirstCreated DATETIME,
    LastUpdated DATETIME,
    
    -- URLs and images
    CompanyLogoURL NVARCHAR(500),
    ProfileImages NVARCHAR(MAX);

PRINT 'Enhanced columns added successfully!';
PRINT '';

-- Create indexes for better search performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodXSuppliers_KosherCert')
    CREATE INDEX IX_FoodXSuppliers_KosherCert ON FoodXSuppliers(KosherCertification);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodXSuppliers_YearFounded')
    CREATE INDEX IX_FoodXSuppliers_YearFounded ON FoodXSuppliers(YearFounded);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodXSuppliers_SupplierCode')
    CREATE INDEX IX_FoodXSuppliers_SupplierCode ON FoodXSuppliers(SupplierCode);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FoodXSuppliers_PrimaryEmail')
    CREATE INDEX IX_FoodXSuppliers_PrimaryEmail ON FoodXSuppliers(PrimaryEmail);

PRINT 'Indexes created for enhanced columns';
PRINT '';
PRINT 'Database schema updated for enhanced supplier data!';
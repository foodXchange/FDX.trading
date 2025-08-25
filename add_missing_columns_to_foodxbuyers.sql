-- Add missing columns to FoodXBuyers table
ALTER TABLE FoodXBuyers
ADD AccountManager NVARCHAR(100) NULL,
    Address NVARCHAR(200) NULL,
    AnnualVolume INT NULL,
    ContactEmail NVARCHAR(200) NULL,
    DistributionCenter NVARCHAR(200) NULL,
    ImportCountries NVARCHAR(100) NULL,
    KeyContact NVARCHAR(200) NULL,
    LastContact DATETIME2 NULL,
    MainCategory NVARCHAR(100) NULL,
    Mobile NVARCHAR(50) NULL,
    Phone NVARCHAR(50) NULL,
    PreferredSuppliers NVARCHAR(100) NULL,
    ProcurementContact NVARCHAR(200) NULL,
    Status NVARCHAR(100) NULL,
    SubCategories NVARCHAR(500) NULL,
    TargetMarket NVARCHAR(100) NULL,
    Title NVARCHAR(100) NULL;
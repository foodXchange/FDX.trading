-- Check if ProductBriefs table exists
IF OBJECT_ID('ProductBriefs', 'U') IS NOT NULL
    SELECT 'ProductBriefs table exists'
ELSE
BEGIN
    -- Create the ProductBriefs table
    CREATE TABLE [ProductBriefs] (
        [Id] int NOT NULL IDENTITY,
        [ProductName] nvarchar(200) NOT NULL,
        [Category] nvarchar(100) NOT NULL,
        [BenchmarkBrand] nvarchar(200) NULL,
        [PackageSize] nvarchar(100) NULL,
        [StorageType] nvarchar(100) NULL,
        [CountryOfOrigin] nvarchar(100) NULL,
        [IsKosher] bit NOT NULL,
        [KosherOrganization] nvarchar(200) NULL,
        [KosherDetails] nvarchar(500) NULL,
        [SpecialAttributes] nvarchar(500) NULL,
        [AdditionalNotes] nvarchar(2000) NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_ProductBriefs] PRIMARY KEY ([Id])
    );
    SELECT 'ProductBriefs table created successfully'
END
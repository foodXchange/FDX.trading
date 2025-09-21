-- Add migration history entries to skip already applied migrations
IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20250921030006_InitialCreate')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20250921030006_InitialCreate', '9.0.0');
END;

IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20250921031650_AddCsvUploadTables')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20250921031650_AddCsvUploadTables', '9.0.0');
END;

-- Create ProductBriefs table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductBriefs')
BEGIN
    CREATE TABLE [ProductBriefs] (
        [Id] int NOT NULL IDENTITY,
        [ProductName] nvarchar(200) NOT NULL,
        [Category] nvarchar(100) NOT NULL,
        [BenchmarkBrandReference] nvarchar(200) NULL,
        [PackageSize] nvarchar(100) NULL,
        [StorageRequirements] nvarchar(200) NULL,
        [CountryOfOrigin] nvarchar(100) NULL,
        [IsKosherCertified] bit NOT NULL,
        [KosherOrganization] nvarchar(100) NULL,
        [KosherSymbol] nvarchar(100) NULL,
        [SpecialAttributes] nvarchar(500) NULL,
        [AdditionalNotes] nvarchar(2000) NULL,
        [CreatedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedDate] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [Status] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_ProductBriefs] PRIMARY KEY ([Id])
    );
END;

-- Add migration history for ProductBrief tables
IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20250921075210_AddProductBriefTable')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20250921075210_AddProductBriefTable', '9.0.0');
END;

IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20250921075211_AddProductBriefs')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20250921075211_AddProductBriefs', '9.0.0');
END;

PRINT 'ProductBrief migration applied successfully!';
BEGIN TRANSACTION;
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

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250921075210_AddProductBriefTable', N'9.0.9');

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

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250921075211_AddProductBriefs', N'9.0.9');

COMMIT;
GO


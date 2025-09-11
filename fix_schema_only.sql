-- Fix Schema Issues - Add Missing Tables and Columns Only
-- This script safely adds only the missing schema elements

-- =====================================================
-- 1. ADD MISSING COLUMNS TO ORDERS TABLE
-- =====================================================

-- Check and add missing columns to Orders table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'AutoConfirmEnabled')
BEGIN
    ALTER TABLE Orders ADD AutoConfirmEnabled bit NOT NULL DEFAULT(0);
    PRINT 'Added AutoConfirmEnabled column to Orders table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'CancellationReason')
BEGIN
    ALTER TABLE Orders ADD CancellationReason nvarchar(500) NULL;
    PRINT 'Added CancellationReason column to Orders table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'CancelledAt')
BEGIN
    ALTER TABLE Orders ADD CancelledAt datetime2 NULL;
    PRINT 'Added CancelledAt column to Orders table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'CommissionAmount')
BEGIN
    ALTER TABLE Orders ADD CommissionAmount decimal(18,2) NOT NULL DEFAULT(0);
    PRINT 'Added CommissionAmount column to Orders table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'ConfirmedAt')
BEGIN
    ALTER TABLE Orders ADD ConfirmedAt datetime2 NULL;
    PRINT 'Added ConfirmedAt column to Orders table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    ALTER TABLE Orders ADD IsDeleted bit NOT NULL DEFAULT(0);
    PRINT 'Added IsDeleted column to Orders table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'IsRecurring')
BEGIN
    ALTER TABLE Orders ADD IsRecurring bit NOT NULL DEFAULT(0);
    PRINT 'Added IsRecurring column to Orders table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'RecurringOrderTemplateId')
BEGIN
    ALTER TABLE Orders ADD RecurringOrderTemplateId int NULL;
    PRINT 'Added RecurringOrderTemplateId column to Orders table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'RecurringSequence')
BEGIN
    ALTER TABLE Orders ADD RecurringSequence nvarchar(50) NULL;
    PRINT 'Added RecurringSequence column to Orders table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'SubTotal')
BEGIN
    ALTER TABLE Orders ADD SubTotal decimal(18,2) NOT NULL DEFAULT(0);
    PRINT 'Added SubTotal column to Orders table';
END

-- =====================================================
-- 2. CREATE SHIPMENTS TABLE IF NOT EXISTS
-- =====================================================

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Shipments')
BEGIN
    CREATE TABLE [Shipments] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [OrderId] int NOT NULL,
        [ShipmentNumber] nvarchar(50) NOT NULL,
        [Status] nvarchar(50) NOT NULL DEFAULT('Pending'),
        [Carrier] nvarchar(100) NULL,
        [TrackingNumber] nvarchar(100) NULL,
        [ContainerNumber] nvarchar(100) NULL,
        [OriginAddress] nvarchar(500) NULL,
        [DestinationAddress] nvarchar(500) NULL,
        [ShippingAddress] nvarchar(500) NULL,
        [EstimatedDeliveryDate] datetime2 NULL,
        [EstimatedArrivalDate] datetime2 NULL,
        [ActualDeliveryDate] datetime2 NULL,
        [ActualArrivalDate] datetime2 NULL,
        [DispatchDate] datetime2 NULL,
        [ApprovalDate] datetime2 NULL,
        [InspectionDate] datetime2 NULL,
        [CurrentLocation] nvarchar(200) NULL,
        [ShipmentValue] decimal(18,2) NULL,
        [InsuranceValue] decimal(18,2) NULL,
        [RequiresInspection] bit NOT NULL DEFAULT(0),
        [DelayNotificationSent] bit NOT NULL DEFAULT(0),
        [Notes] nvarchar(1000) NULL,
        [IsDeleted] bit NOT NULL DEFAULT(0),
        [CreatedAt] datetime2 NOT NULL DEFAULT(GETUTCDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT(GETUTCDATE()),
        [CreatedBy] nvarchar(100) NULL,
        [UpdatedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_Shipments] PRIMARY KEY ([Id])
    );
    
    -- Add foreign key constraint
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Orders')
    BEGIN
        ALTER TABLE [Shipments] ADD CONSTRAINT [FK_Shipments_Orders_OrderId] 
            FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE;
    END
    
    PRINT 'Created Shipments table';
END

-- =====================================================
-- 3. CREATE SHIPMENTITEMS TABLE IF NOT EXISTS
-- =====================================================

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ShipmentItems')
BEGIN
    CREATE TABLE [ShipmentItems] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [ShipmentId] int NOT NULL,
        [ProductId] int NOT NULL,
        [ProductName] nvarchar(200) NOT NULL,
        [SKU] nvarchar(100) NULL,
        [QuantityShipped] decimal(18,2) NOT NULL,
        [QuantityReceived] decimal(18,2) NULL,
        [QuantityAccepted] decimal(18,2) NULL,
        [QuantityRejected] decimal(18,2) NULL,
        [Unit] nvarchar(50) NOT NULL DEFAULT('KG'),
        [UnitPrice] decimal(18,2) NOT NULL,
        [BatchNumber] nvarchar(100) NULL,
        [SerialNumbers] nvarchar(max) NULL,
        [ExpiryDate] datetime2 NULL,
        [QualityGrade] nvarchar(50) NULL,
        [QualityNotes] nvarchar(500) NULL,
        [PackageType] nvarchar(100) NULL,
        [Temperature] decimal(5,2) NULL,
        [Humidity] decimal(5,2) NULL,
        [Origin] nvarchar(100) NULL,
        [CertificationNumbers] nvarchar(500) NULL,
        [DamageNotes] nvarchar(500) NULL,
        [RejectionReason] nvarchar(500) NULL,
        [IsDeleted] bit NOT NULL DEFAULT(0),
        [CreatedAt] datetime2 NOT NULL DEFAULT(GETUTCDATE()),
        CONSTRAINT [PK_ShipmentItems] PRIMARY KEY ([Id])
    );
    
    -- Add foreign key constraints
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Shipments')
    BEGIN
        ALTER TABLE [ShipmentItems] ADD CONSTRAINT [FK_ShipmentItems_Shipments_ShipmentId] 
            FOREIGN KEY ([ShipmentId]) REFERENCES [Shipments] ([Id]) ON DELETE CASCADE;
    END
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products')
    BEGIN
        ALTER TABLE [ShipmentItems] ADD CONSTRAINT [FK_ShipmentItems_Products_ProductId] 
            FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE;
    END
    
    PRINT 'Created ShipmentItems table';
END

-- =====================================================
-- 4. CREATE ORDERITEMS TABLE IF NOT EXISTS
-- =====================================================

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrderItems')
BEGIN
    CREATE TABLE [OrderItems] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [OrderId] int NOT NULL,
        [ProductId] int NOT NULL,
        [ProductName] nvarchar(200) NOT NULL,
        [SKU] nvarchar(100) NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [Unit] nvarchar(50) NOT NULL DEFAULT('KG'),
        [UnitPrice] decimal(18,2) NOT NULL,
        [TotalPrice] decimal(18,2) NOT NULL,
        [DiscountPercent] decimal(5,2) NULL,
        [DiscountAmount] decimal(18,2) NULL,
        [TaxPercent] decimal(5,2) NULL,
        [TaxAmount] decimal(18,2) NULL,
        [Notes] nvarchar(500) NULL,
        [Status] nvarchar(50) NOT NULL DEFAULT('Pending'),
        [IsDeleted] bit NOT NULL DEFAULT(0),
        [CreatedAt] datetime2 NOT NULL DEFAULT(GETUTCDATE()),
        CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id])
    );
    
    -- Add foreign key constraints
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Orders')
    BEGIN
        ALTER TABLE [OrderItems] ADD CONSTRAINT [FK_OrderItems_Orders_OrderId] 
            FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE;
    END
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products')
    BEGIN
        ALTER TABLE [OrderItems] ADD CONSTRAINT [FK_OrderItems_Products_ProductId] 
            FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE;
    END
    
    PRINT 'Created OrderItems table';
END

-- =====================================================
-- 5. CREATE RECURRINGORDERTEMPLATES TABLE IF NOT EXISTS
-- =====================================================

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RecurringOrderTemplates')
BEGIN
    CREATE TABLE [RecurringOrderTemplates] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [TemplateName] nvarchar(100) NOT NULL,
        [TemplateNumber] nvarchar(50) NOT NULL,
        [BuyerId] int NOT NULL,
        [SupplierId] int NOT NULL,
        [Status] nvarchar(50) NOT NULL DEFAULT('Active'),
        [Frequency] nvarchar(50) NOT NULL DEFAULT('Monthly'),
        [FrequencyValue] int NOT NULL DEFAULT(1),
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NULL,
        [NextGenerationDate] datetime2 NULL,
        [Currency] nvarchar(3) NOT NULL DEFAULT('EUR'),
        [PaymentTerms] nvarchar(50) NOT NULL DEFAULT('NET30'),
        [ShippingAddress] nvarchar(500) NULL,
        [BillingAddress] nvarchar(500) NULL,
        [Notes] nvarchar(1000) NULL,
        [InternalNotes] nvarchar(1000) NULL,
        [AutoGenerate] bit NOT NULL DEFAULT(1),
        [RequireApproval] bit NOT NULL DEFAULT(0),
        [GenerationDaysInAdvance] int NOT NULL DEFAULT(7),
        [Version] int NOT NULL DEFAULT(1),
        [IsCurrentVersion] bit NOT NULL DEFAULT(1),
        [PreviousVersionId] int NULL,
        [TotalOrdersGenerated] int NOT NULL DEFAULT(0),
        [TotalValueGenerated] decimal(18,2) NOT NULL DEFAULT(0),
        [LastOrderGenerated] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT(GETUTCDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT(GETUTCDATE()),
        [CreatedBy] nvarchar(100) NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [IsDeleted] bit NOT NULL DEFAULT(0),
        CONSTRAINT [PK_RecurringOrderTemplates] PRIMARY KEY ([Id])
    );
    
    PRINT 'Created RecurringOrderTemplates table';
END

-- =====================================================
-- 6. CREATE RECURRINGORDERITEMS TABLE IF NOT EXISTS
-- =====================================================

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RecurringOrderItems')
BEGIN
    CREATE TABLE [RecurringOrderItems] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [RecurringOrderTemplateId] int NOT NULL,
        [ProductId] int NOT NULL,
        [ProductName] nvarchar(200) NOT NULL,
        [SKU] nvarchar(100) NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [Unit] nvarchar(50) NOT NULL DEFAULT('KG'),
        [UnitPrice] decimal(18,2) NOT NULL,
        [DiscountPercent] decimal(5,2) NULL,
        [Notes] nvarchar(500) NULL,
        [AllowQuantityVariation] bit NOT NULL DEFAULT(0),
        [MaxQuantityVariationPercent] decimal(5,2) NULL,
        [AllowPriceVariation] bit NOT NULL DEFAULT(0),
        [MaxPriceVariationPercent] decimal(5,2) NULL,
        [HasSeasonalAdjustments] bit NOT NULL DEFAULT(0),
        [SpringAdjustmentPercent] decimal(5,2) NULL,
        [SummerAdjustmentPercent] decimal(5,2) NULL,
        [AutumnAdjustmentPercent] decimal(5,2) NULL,
        [WinterAdjustmentPercent] decimal(5,2) NULL,
        [SortOrder] int NOT NULL DEFAULT(0),
        [CreatedAt] datetime2 NOT NULL DEFAULT(GETUTCDATE()),
        CONSTRAINT [PK_RecurringOrderItems] PRIMARY KEY ([Id])
    );
    
    -- Add foreign key constraints
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RecurringOrderTemplates')
    BEGIN
        ALTER TABLE [RecurringOrderItems] ADD CONSTRAINT [FK_RecurringOrderItems_RecurringOrderTemplates_RecurringOrderTemplateId] 
            FOREIGN KEY ([RecurringOrderTemplateId]) REFERENCES [RecurringOrderTemplates] ([Id]) ON DELETE CASCADE;
    END
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products')
    BEGIN
        ALTER TABLE [RecurringOrderItems] ADD CONSTRAINT [FK_RecurringOrderItems_Products_ProductId] 
            FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE;
    END
    
    PRINT 'Created RecurringOrderItems table';
END

-- =====================================================
-- 7. CREATE RECURRINGORDERHISTORY TABLE IF NOT EXISTS
-- =====================================================

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RecurringOrderHistory')
BEGIN
    CREATE TABLE [RecurringOrderHistory] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [RecurringOrderTemplateId] int NOT NULL,
        [Action] nvarchar(50) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [GeneratedOrderId] int NULL,
        [PerformedBy] nvarchar(100) NULL,
        [PerformedAt] datetime2 NOT NULL DEFAULT(GETUTCDATE()),
        [Metadata] nvarchar(max) NULL,
        CONSTRAINT [PK_RecurringOrderHistory] PRIMARY KEY ([Id])
    );
    
    -- Add foreign key constraints
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RecurringOrderTemplates')
    BEGIN
        ALTER TABLE [RecurringOrderHistory] ADD CONSTRAINT [FK_RecurringOrderHistory_RecurringOrderTemplates_RecurringOrderTemplateId] 
            FOREIGN KEY ([RecurringOrderTemplateId]) REFERENCES [RecurringOrderTemplates] ([Id]) ON DELETE CASCADE;
    END
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Orders')
    BEGIN
        ALTER TABLE [RecurringOrderHistory] ADD CONSTRAINT [FK_RecurringOrderHistory_Orders_GeneratedOrderId] 
            FOREIGN KEY ([GeneratedOrderId]) REFERENCES [Orders] ([Id]);
    END
    
    PRINT 'Created RecurringOrderHistory table';
END

-- =====================================================
-- 8. ADD FOREIGN KEY FOR ORDERS.RECURRINGORDERTEMPLATEID
-- =====================================================

-- Add foreign key constraint for RecurringOrderTemplateId in Orders table
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'RecurringOrderTemplateId') 
   AND EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RecurringOrderTemplates')
   AND NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_Orders_RecurringOrderTemplates_RecurringOrderTemplateId')
BEGIN
    ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_RecurringOrderTemplates_RecurringOrderTemplateId] 
        FOREIGN KEY ([RecurringOrderTemplateId]) REFERENCES [RecurringOrderTemplates] ([Id]);
    PRINT 'Added foreign key constraint for Orders.RecurringOrderTemplateId';
END

-- =====================================================
-- 9. ADD INDEXES FOR PERFORMANCE
-- =====================================================

-- Index on Orders.Status for faster queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_Status')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Orders_Status ON Orders(Status) INCLUDE (OrderDate, TotalAmount);
    PRINT 'Created index IX_Orders_Status';
END

-- Index on Shipments.Status for faster queries
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Shipments')
   AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Shipments_Status')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Shipments_Status ON Shipments(Status, EstimatedDeliveryDate);
    PRINT 'Created index IX_Shipments_Status';
END

-- Index on RecurringOrderTemplates.Status and NextGenerationDate
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RecurringOrderTemplates')
   AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RecurringOrderTemplates_Status_NextGeneration')
BEGIN
    CREATE NONCLUSTERED INDEX IX_RecurringOrderTemplates_Status_NextGeneration 
        ON RecurringOrderTemplates(Status, NextGenerationDate) INCLUDE (AutoGenerate);
    PRINT 'Created index IX_RecurringOrderTemplates_Status_NextGeneration';
END

PRINT 'Schema fix completed successfully!';
-- Pricing & Negotiation Tables Migration
-- Fixed version without SupplierQuotes dependency

-- Check and create BuyerSpecificPricing table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BuyerSpecificPricing')
BEGIN
    CREATE TABLE BuyerSpecificPricing (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        BuyerId INT NOT NULL,
        SupplierId INT NOT NULL,
        ProductCode NVARCHAR(100),
        ProductName NVARCHAR(200) NOT NULL,
        NegotiatedUnitPrice DECIMAL(18,4) NOT NULL,
        Currency NVARCHAR(3) NOT NULL DEFAULT 'EUR',
        MinimumOrderQuantity DECIMAL(18,2),
        Unit NVARCHAR(50) NOT NULL DEFAULT 'KG',
        FreightCostPerUnit DECIMAL(18,4),
        CustomsDutyRate DECIMAL(5,2),
        InsuranceRate DECIMAL(5,2),
        Incoterms NVARCHAR(10),
        PaymentTerms NVARCHAR(200),
        LeadTimeDays INT,
        ValidFrom DATETIME NOT NULL,
        ValidUntil DATETIME NOT NULL,
        ConfidentialityLevel NVARCHAR(50) NOT NULL DEFAULT 'Strict',
        IsConfidential BIT NOT NULL DEFAULT 1,
        LastNegotiatedDate DATETIME,
        Notes NVARCHAR(MAX),
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(100),
        UpdatedBy NVARCHAR(100),
        IsDeleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_BuyerSpecificPricing_Buyer FOREIGN KEY (BuyerId)
            REFERENCES FoodXBuyers(Id),
        CONSTRAINT FK_BuyerSpecificPricing_Supplier FOREIGN KEY (SupplierId)
            REFERENCES FoodXSuppliers(Id)
    );

    CREATE INDEX IX_BuyerSpecificPricing_Buyer ON BuyerSpecificPricing(BuyerId);
    CREATE INDEX IX_BuyerSpecificPricing_Supplier ON BuyerSpecificPricing(SupplierId);
    CREATE INDEX IX_BuyerSpecificPricing_Product ON BuyerSpecificPricing(ProductCode);
    CREATE INDEX IX_BuyerSpecificPricing_ValidDates ON BuyerSpecificPricing(ValidFrom, ValidUntil);

    PRINT 'Created BuyerSpecificPricing table';
END
ELSE
    PRINT 'BuyerSpecificPricing table already exists';
GO

-- Check and create BuyerPriceTiers table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BuyerPriceTiers')
BEGIN
    CREATE TABLE BuyerPriceTiers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        BuyerSpecificPricingId INT NOT NULL,
        MinQuantity DECIMAL(18,2) NOT NULL,
        MaxQuantity DECIMAL(18,2),
        TierPrice DECIMAL(18,4) NOT NULL,
        DiscountPercentage DECIMAL(5,2),
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_BuyerPriceTiers_Pricing FOREIGN KEY (BuyerSpecificPricingId)
            REFERENCES BuyerSpecificPricing(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_BuyerPriceTiers_Pricing ON BuyerPriceTiers(BuyerSpecificPricingId);

    PRINT 'Created BuyerPriceTiers table';
END
ELSE
    PRINT 'BuyerPriceTiers table already exists';
GO

-- Check and create NegotiationHistory table (without SupplierQuotes reference)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NegotiationHistory')
BEGIN
    CREATE TABLE NegotiationHistory (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        NegotiationNumber NVARCHAR(50) NOT NULL,
        BuyerId INT NOT NULL,
        SupplierId INT NOT NULL,
        RFQId INT,
        -- Removed: SupplierQuoteId INT,
        ProductName NVARCHAR(200) NOT NULL,
        ProductCode NVARCHAR(100),
        Quantity DECIMAL(18,2) NOT NULL,
        Unit NVARCHAR(50) NOT NULL DEFAULT 'KG',
        StartedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CompletedAt DATETIME,
        Status NVARCHAR(50) NOT NULL DEFAULT 'InProgress',
        InitialPrice DECIMAL(18,4) NOT NULL,
        FinalAgreedPrice DECIMAL(18,4),
        Currency NVARCHAR(3) NOT NULL DEFAULT 'EUR',
        TotalRounds INT DEFAULT 0,
        EmailCount INT DEFAULT 0,
        PhoneCallCount INT DEFAULT 0,
        MeetingCount INT DEFAULT 0,
        FinalTerms NVARCHAR(1000),
        FinalIncoterms NVARCHAR(100),
        FinalPaymentTerms NVARCHAR(200),
        FinalPriceValidUntil DATETIME,
        BuyerNegotiator NVARCHAR(100),
        SupplierNegotiator NVARCHAR(100),
        InternalNotes NVARCHAR(2000),
        SharedNotes NVARCHAR(2000),
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(100),
        UpdatedBy NVARCHAR(100),
        CONSTRAINT FK_NegotiationHistory_Buyer FOREIGN KEY (BuyerId)
            REFERENCES FoodXBuyers(Id),
        CONSTRAINT FK_NegotiationHistory_Supplier FOREIGN KEY (SupplierId)
            REFERENCES FoodXSuppliers(Id),
        CONSTRAINT FK_NegotiationHistory_RFQ FOREIGN KEY (RFQId)
            REFERENCES RFQs(Id)
    );

    CREATE INDEX IX_NegotiationHistory_Buyer ON NegotiationHistory(BuyerId);
    CREATE INDEX IX_NegotiationHistory_Supplier ON NegotiationHistory(SupplierId);
    CREATE INDEX IX_NegotiationHistory_Status ON NegotiationHistory(Status);
    CREATE INDEX IX_NegotiationHistory_Dates ON NegotiationHistory(StartedAt, CompletedAt);

    PRINT 'Created NegotiationHistory table';
END
ELSE
    PRINT 'NegotiationHistory table already exists';
GO

-- Check and create NegotiationRounds table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NegotiationRounds')
BEGIN
    CREATE TABLE NegotiationRounds (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        NegotiationHistoryId INT NOT NULL,
        RoundNumber INT NOT NULL,
        OccurredAt DATETIME NOT NULL,
        CommunicationMethod NVARCHAR(50) NOT NULL,
        CommunicationReference NVARCHAR(100),
        ProposedPriceBuyer DECIMAL(18,4),
        ProposedPriceSupplier DECIMAL(18,4),
        AgreedPrice DECIMAL(18,4),
        QuantityDiscussed NVARCHAR(500),
        DeliveryTermsDiscussed NVARCHAR(500),
        PaymentTermsDiscussed NVARCHAR(500),
        QualityRequirementsDiscussed NVARCHAR(500),
        DiscussionSummary NVARCHAR(2000),
        BuyerPosition NVARCHAR(1000),
        SupplierPosition NVARCHAR(1000),
        Outcome NVARCHAR(50) NOT NULL,
        NextSteps NVARCHAR(500),
        AttachedDocuments NVARCHAR(1000),
        BuyerParticipants NVARCHAR(200),
        SupplierParticipants NVARCHAR(200),
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(100),
        IsDeleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_NegotiationRounds_History FOREIGN KEY (NegotiationHistoryId)
            REFERENCES NegotiationHistory(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_NegotiationRounds_History ON NegotiationRounds(NegotiationHistoryId);
    CREATE INDEX IX_NegotiationRounds_Date ON NegotiationRounds(OccurredAt);

    PRINT 'Created NegotiationRounds table';
END
ELSE
    PRINT 'NegotiationRounds table already exists';
GO

-- Check and create NegotiationEmailTemplates table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NegotiationEmailTemplates')
BEGIN
    CREATE TABLE NegotiationEmailTemplates (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TemplateName NVARCHAR(100) NOT NULL,
        TemplateType NVARCHAR(50) NOT NULL,
        Subject NVARCHAR(200) NOT NULL,
        Body NVARCHAR(MAX) NOT NULL,
        Variables NVARCHAR(500),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE()
    );

    -- Insert default templates
    INSERT INTO NegotiationEmailTemplates (TemplateName, TemplateType, Subject, Body, Variables)
    VALUES
    ('Initial Price Request', 'InitialOffer', 'Price Quote Request - {ProductName}',
     'Dear {SupplierName},\n\nWe are interested in purchasing {Quantity} {Unit} of {ProductName}.\n\nPlease provide your best pricing and terms.\n\nBest regards,\n{BuyerName}',
     '["{SupplierName}","{ProductName}","{Quantity}","{Unit}","{BuyerName}"]'),

    ('Counter Offer', 'CounterOffer', 'Re: Price Quote - {ProductName}',
     'Dear {SupplierName},\n\nThank you for your quote. We would like to propose {ProposedPrice} {Currency} per {Unit}.\n\nThis is based on {Reason}.\n\nBest regards,\n{BuyerName}',
     '["{SupplierName}","{ProductName}","{ProposedPrice}","{Currency}","{Unit}","{Reason}","{BuyerName}"]'),

    ('Price Acceptance', 'Acceptance', 'Price Agreement - {ProductName}',
     'Dear {SupplierName},\n\nWe are pleased to accept your price of {AgreedPrice} {Currency} per {Unit} for {ProductName}.\n\nNext steps: {NextSteps}\n\nBest regards,\n{BuyerName}',
     '["{SupplierName}","{ProductName}","{AgreedPrice}","{Currency}","{Unit}","{NextSteps}","{BuyerName}"]');

    PRINT 'Created NegotiationEmailTemplates table and inserted default templates';
END
ELSE
    PRINT 'NegotiationEmailTemplates table already exists';
GO

-- Check and create ComplianceVerifications table (without SupplierQuotes reference)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ComplianceVerifications')
BEGIN
    CREATE TABLE ComplianceVerifications (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        VerificationNumber NVARCHAR(50) NOT NULL,
        BuyerId INT NOT NULL,
        SupplierId INT NOT NULL,
        -- Removed: SupplierQuoteId INT,
        RFQId INT,
        ProductName NVARCHAR(200) NOT NULL,
        ProductCode NVARCHAR(100),
        ProductCategory NVARCHAR(100),
        BuyerCountry NVARCHAR(100) NOT NULL,
        RegulatoryAuthority NVARCHAR(500),
        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        ApprovedAt DATETIME,
        RejectedAt DATETIME,
        ApprovedBy NVARCHAR(100),
        RejectionReason NVARCHAR(1000),
        RequiresKosher BIT NOT NULL DEFAULT 0,
        RequiresHalal BIT NOT NULL DEFAULT 0,
        RequiresOrganic BIT NOT NULL DEFAULT 0,
        RequiresGlutenFree BIT NOT NULL DEFAULT 0,
        RequiresNonGMO BIT NOT NULL DEFAULT 0,
        RequiresFairTrade BIT NOT NULL DEFAULT 0,
        RequiresISO22000 BIT NOT NULL DEFAULT 0,
        RequiresHACCP BIT NOT NULL DEFAULT 0,
        RequiresBRC BIT NOT NULL DEFAULT 0,
        RequiresFDA BIT NOT NULL DEFAULT 0,
        OtherRequiredCertifications NVARCHAR(1000),
        RequiresLabTesting BIT NOT NULL DEFAULT 0,
        RequiredTests NVARCHAR(500),
        ValidFrom DATETIME NOT NULL,
        ValidUntil DATETIME NOT NULL,
        IsConditional BIT NOT NULL DEFAULT 0,
        Conditions NVARCHAR(1000),
        ConditionDeadline DATETIME,
        InternalNotes NVARCHAR(2000),
        SupplierNotes NVARCHAR(1000),
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(100),
        UpdatedBy NVARCHAR(100),
        CONSTRAINT FK_ComplianceVerifications_Buyer FOREIGN KEY (BuyerId)
            REFERENCES FoodXBuyers(Id),
        CONSTRAINT FK_ComplianceVerifications_Supplier FOREIGN KEY (SupplierId)
            REFERENCES FoodXSuppliers(Id),
        CONSTRAINT FK_ComplianceVerifications_RFQ FOREIGN KEY (RFQId)
            REFERENCES RFQs(Id)
    );

    CREATE INDEX IX_ComplianceVerifications_Buyer ON ComplianceVerifications(BuyerId);
    CREATE INDEX IX_ComplianceVerifications_Supplier ON ComplianceVerifications(SupplierId);
    CREATE INDEX IX_ComplianceVerifications_Status ON ComplianceVerifications(Status);
    CREATE INDEX IX_ComplianceVerifications_ValidDates ON ComplianceVerifications(ValidFrom, ValidUntil);

    PRINT 'Created ComplianceVerifications table';
END
ELSE
    PRINT 'ComplianceVerifications table already exists';
GO

-- Check and create CertificationDocuments table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CertificationDocuments')
BEGIN
    CREATE TABLE CertificationDocuments (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ComplianceVerificationId INT NOT NULL,
        CertificationType NVARCHAR(100) NOT NULL,
        CertificateName NVARCHAR(200) NOT NULL,
        CertificateNumber NVARCHAR(100),
        IssuingAuthority NVARCHAR(200),
        IssueDate DATETIME NOT NULL,
        ExpiryDate DATETIME NOT NULL,
        DocumentUrl NVARCHAR(500) NOT NULL,
        DocumentHash NVARCHAR(100),
        VerificationStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        VerifiedAt DATETIME,
        VerifiedBy NVARCHAR(100),
        VerificationNotes NVARCHAR(500),
        UploadedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        UploadedBy NVARCHAR(100),
        IsDeleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_CertificationDocuments_Compliance FOREIGN KEY (ComplianceVerificationId)
            REFERENCES ComplianceVerifications(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_CertificationDocuments_Compliance ON CertificationDocuments(ComplianceVerificationId);
    CREATE INDEX IX_CertificationDocuments_Type ON CertificationDocuments(CertificationType);
    CREATE INDEX IX_CertificationDocuments_Status ON CertificationDocuments(VerificationStatus);

    PRINT 'Created CertificationDocuments table';
END
ELSE
    PRINT 'CertificationDocuments table already exists';
GO

-- Check and create ComplianceChecklistItems table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ComplianceChecklistItems')
BEGIN
    CREATE TABLE ComplianceChecklistItems (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ComplianceVerificationId INT NOT NULL,
        RequirementName NVARCHAR(200) NOT NULL,
        RequirementDescription NVARCHAR(500),
        Category NVARCHAR(50) NOT NULL,
        IsMandatory BIT NOT NULL DEFAULT 1,
        IsCompleted BIT NOT NULL DEFAULT 0,
        CompletedAt DATETIME,
        CompletedBy NVARCHAR(100),
        Evidence NVARCHAR(500),
        Notes NVARCHAR(500),
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_ComplianceChecklistItems_Compliance FOREIGN KEY (ComplianceVerificationId)
            REFERENCES ComplianceVerifications(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_ComplianceChecklistItems_Compliance ON ComplianceChecklistItems(ComplianceVerificationId);
    CREATE INDEX IX_ComplianceChecklistItems_Category ON ComplianceChecklistItems(Category);

    PRINT 'Created ComplianceChecklistItems table';
END
ELSE
    PRINT 'ComplianceChecklistItems table already exists';
GO

-- Check and create LabTestResults table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LabTestResults')
BEGIN
    CREATE TABLE LabTestResults (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ComplianceVerificationId INT NOT NULL,
        TestType NVARCHAR(100) NOT NULL,
        TestName NVARCHAR(200) NOT NULL,
        TestMethod NVARCHAR(100),
        Laboratory NVARCHAR(200) NOT NULL,
        LabCertification NVARCHAR(100),
        TestDate DATETIME NOT NULL,
        SampleReference NVARCHAR(100),
        Result NVARCHAR(500) NOT NULL,
        Unit NVARCHAR(100),
        AcceptableRange NVARCHAR(100),
        PassedTest BIT NOT NULL,
        ReportUrl NVARCHAR(500) NOT NULL,
        ReportNumber NVARCHAR(100),
        UploadedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        UploadedBy NVARCHAR(100),
        IsDeleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_LabTestResults_Compliance FOREIGN KEY (ComplianceVerificationId)
            REFERENCES ComplianceVerifications(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_LabTestResults_Compliance ON LabTestResults(ComplianceVerificationId);
    CREATE INDEX IX_LabTestResults_TestType ON LabTestResults(TestType);
    CREATE INDEX IX_LabTestResults_PassedTest ON LabTestResults(PassedTest);

    PRINT 'Created LabTestResults table';
END
ELSE
    PRINT 'LabTestResults table already exists';
GO

PRINT 'Migration completed successfully!';
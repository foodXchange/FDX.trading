-- =============================================
-- FoodX Project Management System Database Schema
-- =============================================

-- =============================================
-- 1. Projects Table - Main project entity
-- =============================================
CREATE TABLE Projects (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProjectNumber NVARCHAR(50) NOT NULL UNIQUE,
    ProjectName NVARCHAR(200) NOT NULL,
    ProjectType NVARCHAR(50) NOT NULL CHECK (ProjectType IN ('BuyerRFQ', 'SupplierOffer', 'DirectNegotiation')),
    Status NVARCHAR(50) NOT NULL DEFAULT 'Draft',
    Priority NVARCHAR(20) DEFAULT 'Normal' CHECK (Priority IN ('Low', 'Normal', 'High', 'Urgent')),
    
    -- Initiator Information
    InitiatorType NVARCHAR(20) NOT NULL CHECK (InitiatorType IN ('Buyer', 'Supplier')),
    InitiatorUserId INT NOT NULL,
    InitiatorCompanyId INT NULL,
    
    -- Project Details
    Description NVARCHAR(MAX),
    TotalValue DECIMAL(18,2) NULL,
    Currency NVARCHAR(3) DEFAULT 'USD',
    
    -- Dates
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    StartDate DATETIME2 NULL,
    DueDate DATETIME2 NULL,
    CompletedDate DATETIME2 NULL,
    
    -- Tracking
    CreatedBy INT NOT NULL,
    UpdatedBy INT NULL,
    IsActive BIT DEFAULT 1,
    
    FOREIGN KEY (InitiatorUserId) REFERENCES Users(Id),
    FOREIGN KEY (InitiatorCompanyId) REFERENCES Companies(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);

-- =============================================
-- 2. Project Stages - Workflow stages for projects
-- =============================================
CREATE TABLE ProjectStages (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    StageName NVARCHAR(100) NOT NULL,
    StageOrder INT NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Pending' CHECK (Status IN ('Pending', 'InProgress', 'Completed', 'Skipped', 'Failed')),
    
    -- Stage Details
    Description NVARCHAR(500),
    RequiredApprovals INT DEFAULT 0,
    CurrentApprovals INT DEFAULT 0,
    
    -- Dates
    StartedAt DATETIME2 NULL,
    CompletedAt DATETIME2 NULL,
    DueDate DATETIME2 NULL,
    
    -- Assignment
    AssignedToUserId INT NULL,
    AssignedToTeam NVARCHAR(100) NULL,
    
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE,
    FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id)
);

-- =============================================
-- 3. Project Team Members - Team assignments
-- =============================================
CREATE TABLE ProjectTeamMembers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    UserId INT NOT NULL,
    Role NVARCHAR(100) NOT NULL,
    Permissions NVARCHAR(50) DEFAULT 'View' CHECK (Permissions IN ('View', 'Edit', 'Approve', 'Admin')),
    
    -- Assignment Details
    AssignedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    AssignedBy INT NOT NULL,
    IsActive BIT DEFAULT 1,
    Notes NVARCHAR(500),
    
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (AssignedBy) REFERENCES Users(Id),
    UNIQUE (ProjectId, UserId, Role)
);

-- =============================================
-- 4. RFQs - Request for Quotation
-- =============================================
CREATE TABLE RFQs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    RFQNumber NVARCHAR(50) NOT NULL UNIQUE,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    
    -- Buyer Information
    BuyerId INT NOT NULL,
    BuyerCompanyId INT NULL,
    
    -- RFQ Details
    Category NVARCHAR(100),
    DeliveryLocation NVARCHAR(500),
    DeliveryTerms NVARCHAR(200),
    PaymentTerms NVARCHAR(200),
    
    -- Dates
    IssuedDate DATETIME2 DEFAULT SYSUTCDATETIME(),
    SubmissionDeadline DATETIME2 NOT NULL,
    QuotationValidityPeriod INT DEFAULT 30, -- days
    
    -- Status
    Status NVARCHAR(50) DEFAULT 'Draft' CHECK (Status IN ('Draft', 'Published', 'Closed', 'Awarded', 'Cancelled')),
    IsPublic BIT DEFAULT 0,
    
    -- Budget
    EstimatedBudget DECIMAL(18,2) NULL,
    ShowBudgetToSuppliers BIT DEFAULT 0,
    
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    FOREIGN KEY (BuyerId) REFERENCES Buyers(Id),
    FOREIGN KEY (BuyerCompanyId) REFERENCES Companies(Id)
);

-- =============================================
-- 5. RFQ Items - Line items in RFQ
-- =============================================
CREATE TABLE RFQItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RFQId INT NOT NULL,
    ItemNumber INT NOT NULL,
    
    -- Product Information
    ProductName NVARCHAR(200) NOT NULL,
    ProductCategory NVARCHAR(100),
    Description NVARCHAR(MAX),
    Specifications NVARCHAR(MAX),
    
    -- Quantity and Unit
    Quantity DECIMAL(18,3) NOT NULL,
    Unit NVARCHAR(50) NOT NULL,
    
    -- Quality Requirements
    QualityStandards NVARCHAR(500),
    CertificationsRequired NVARCHAR(500),
    
    -- Delivery
    RequiredDeliveryDate DATETIME2,
    DeliveryFrequency NVARCHAR(100), -- For recurring orders
    
    FOREIGN KEY (RFQId) REFERENCES RFQs(Id) ON DELETE CASCADE
);

-- =============================================
-- 6. Quotes - Supplier responses to RFQs
-- =============================================
CREATE TABLE Quotes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RFQId INT NOT NULL,
    ProjectId INT NOT NULL,
    QuoteNumber NVARCHAR(50) NOT NULL UNIQUE,
    
    -- Supplier Information
    SupplierId INT NOT NULL,
    SupplierCompanyId INT NULL,
    
    -- Quote Details
    TotalAmount DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(3) DEFAULT 'USD',
    ValidUntil DATETIME2 NOT NULL,
    
    -- Terms
    DeliveryTerms NVARCHAR(500),
    PaymentTerms NVARCHAR(500),
    WarrantyTerms NVARCHAR(500),
    
    -- Status
    Status NVARCHAR(50) DEFAULT 'Draft' CHECK (Status IN ('Draft', 'Submitted', 'UnderReview', 'Shortlisted', 'Accepted', 'Rejected', 'Withdrawn')),
    
    -- Evaluation
    TechnicalScore DECIMAL(5,2) NULL,
    CommercialScore DECIMAL(5,2) NULL,
    OverallScore DECIMAL(5,2) NULL,
    EvaluationNotes NVARCHAR(MAX),
    
    -- Timestamps
    SubmittedAt DATETIME2 NULL,
    ReviewedAt DATETIME2 NULL,
    DecisionAt DATETIME2 NULL,
    
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    
    FOREIGN KEY (RFQId) REFERENCES RFQs(Id),
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id),
    FOREIGN KEY (SupplierCompanyId) REFERENCES Companies(Id)
);

-- =============================================
-- 7. Quote Items - Line items in quotes
-- =============================================
CREATE TABLE QuoteItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    QuoteId INT NOT NULL,
    RFQItemId INT NOT NULL,
    
    -- Pricing
    UnitPrice DECIMAL(18,4) NOT NULL,
    Quantity DECIMAL(18,3) NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    
    -- Product Details
    OfferedProductName NVARCHAR(200),
    OfferedProductDescription NVARCHAR(MAX),
    
    -- Delivery
    DeliveryLeadTime INT, -- days
    AvailableQuantity DECIMAL(18,3),
    
    -- Notes
    Notes NVARCHAR(MAX),
    
    FOREIGN KEY (QuoteId) REFERENCES Quotes(Id) ON DELETE CASCADE,
    FOREIGN KEY (RFQItemId) REFERENCES RFQItems(Id)
);

-- =============================================
-- 8. Supplier Offers - Proactive supplier offerings
-- =============================================
CREATE TABLE SupplierOffers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NULL,
    OfferNumber NVARCHAR(50) NOT NULL UNIQUE,
    
    -- Supplier Information
    SupplierId INT NOT NULL,
    SupplierCompanyId INT NULL,
    
    -- Offer Details
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Category NVARCHAR(100),
    
    -- Validity
    ValidFrom DATETIME2 DEFAULT SYSUTCDATETIME(),
    ValidUntil DATETIME2 NOT NULL,
    
    -- Terms
    MinimumOrderValue DECIMAL(18,2),
    DeliveryTerms NVARCHAR(500),
    PaymentTerms NVARCHAR(500),
    
    -- Status
    Status NVARCHAR(50) DEFAULT 'Active' CHECK (Status IN ('Draft', 'Active', 'Expired', 'Withdrawn')),
    IsPublic BIT DEFAULT 1,
    
    -- Targeting
    TargetBuyerTypes NVARCHAR(500), -- JSON array of buyer types
    TargetRegions NVARCHAR(500), -- JSON array of regions
    
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id),
    FOREIGN KEY (SupplierCompanyId) REFERENCES Companies(Id)
);

-- =============================================
-- 9. Project Documents - Document attachments
-- =============================================
CREATE TABLE ProjectDocuments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    DocumentType NVARCHAR(100) NOT NULL,
    DocumentName NVARCHAR(200) NOT NULL,
    
    -- File Information
    FileName NVARCHAR(500) NOT NULL,
    FilePath NVARCHAR(1000) NOT NULL,
    FileSize BIGINT,
    MimeType NVARCHAR(100),
    
    -- Metadata
    Description NVARCHAR(500),
    Version INT DEFAULT 1,
    IsLatestVersion BIT DEFAULT 1,
    
    -- Security
    IsConfidential BIT DEFAULT 0,
    AccessLevel NVARCHAR(50) DEFAULT 'Team' CHECK (AccessLevel IN ('Public', 'Team', 'Restricted')),
    
    -- Tracking
    UploadedBy INT NOT NULL,
    UploadedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE,
    FOREIGN KEY (UploadedBy) REFERENCES Users(Id)
);

-- =============================================
-- 10. Project Activities - Activity log
-- =============================================
CREATE TABLE ProjectActivities (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    ActivityType NVARCHAR(100) NOT NULL,
    ActivityDescription NVARCHAR(MAX) NOT NULL,
    
    -- Context
    EntityType NVARCHAR(50), -- 'RFQ', 'Quote', 'Document', etc.
    EntityId INT,
    
    -- User Information
    UserId INT NOT NULL,
    UserRole NVARCHAR(100),
    
    -- Metadata
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    
    -- Timestamp
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- =============================================
-- 11. Project Messages - Internal communications
-- =============================================
CREATE TABLE ProjectMessages (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    ParentMessageId INT NULL,
    
    -- Message Details
    Subject NVARCHAR(200),
    MessageBody NVARCHAR(MAX) NOT NULL,
    MessageType NVARCHAR(50) DEFAULT 'General' CHECK (MessageType IN ('General', 'Question', 'Clarification', 'Update', 'Alert')),
    
    -- Sender
    SenderId INT NOT NULL,
    SenderRole NVARCHAR(100),
    
    -- Recipients
    RecipientType NVARCHAR(50) DEFAULT 'Team' CHECK (RecipientType IN ('Team', 'Specific', 'Buyer', 'Supplier')),
    RecipientIds NVARCHAR(500), -- JSON array of user IDs
    
    -- Status
    IsRead BIT DEFAULT 0,
    IsImportant BIT DEFAULT 0,
    
    -- Timestamps
    SentAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    ReadAt DATETIME2 NULL,
    
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE,
    FOREIGN KEY (ParentMessageId) REFERENCES ProjectMessages(Id),
    FOREIGN KEY (SenderId) REFERENCES Users(Id)
);

-- =============================================
-- 12. Workflow Templates - Predefined workflows
-- =============================================
CREATE TABLE WorkflowTemplates (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TemplateName NVARCHAR(100) NOT NULL UNIQUE,
    TemplateType NVARCHAR(50) NOT NULL CHECK (TemplateType IN ('BuyerRFQ', 'SupplierOffer', 'DirectNegotiation')),
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
);

-- =============================================
-- 13. Workflow Stages - Stages in templates
-- =============================================
CREATE TABLE WorkflowStages (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TemplateId INT NOT NULL,
    StageName NVARCHAR(100) NOT NULL,
    StageOrder INT NOT NULL,
    
    -- Stage Configuration
    Description NVARCHAR(500),
    DefaultDuration INT, -- days
    IsMandatory BIT DEFAULT 1,
    RequiresApproval BIT DEFAULT 0,
    ApprovalLevel INT DEFAULT 1,
    
    -- Auto-assignment
    DefaultAssigneeRole NVARCHAR(100),
    
    FOREIGN KEY (TemplateId) REFERENCES WorkflowTemplates(Id) ON DELETE CASCADE
);

-- =============================================
-- 14. Project Notifications - Notification queue
-- =============================================
CREATE TABLE ProjectNotifications (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    UserId INT NOT NULL,
    
    -- Notification Details
    NotificationType NVARCHAR(100) NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX),
    
    -- Priority and Status
    Priority NVARCHAR(20) DEFAULT 'Normal' CHECK (Priority IN ('Low', 'Normal', 'High', 'Urgent')),
    IsRead BIT DEFAULT 0,
    IsEmailSent BIT DEFAULT 0,
    
    -- Action
    ActionRequired BIT DEFAULT 0,
    ActionUrl NVARCHAR(500),
    ActionDeadline DATETIME2 NULL,
    
    -- Timestamps
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    ReadAt DATETIME2 NULL,
    EmailSentAt DATETIME2 NULL,
    
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- =============================================
-- Create Indexes for Performance
-- =============================================
CREATE INDEX IX_Projects_Status ON Projects(Status);
CREATE INDEX IX_Projects_InitiatorUserId ON Projects(InitiatorUserId);
CREATE INDEX IX_Projects_CreatedAt ON Projects(CreatedAt DESC);

CREATE INDEX IX_ProjectStages_ProjectId_Status ON ProjectStages(ProjectId, Status);
CREATE INDEX IX_ProjectTeamMembers_UserId ON ProjectTeamMembers(UserId);

CREATE INDEX IX_RFQs_Status_SubmissionDeadline ON RFQs(Status, SubmissionDeadline);
CREATE INDEX IX_RFQs_BuyerId ON RFQs(BuyerId);

CREATE INDEX IX_Quotes_RFQId_Status ON Quotes(RFQId, Status);
CREATE INDEX IX_Quotes_SupplierId ON Quotes(SupplierId);

CREATE INDEX IX_SupplierOffers_Status_ValidUntil ON SupplierOffers(Status, ValidUntil);
CREATE INDEX IX_SupplierOffers_SupplierId ON SupplierOffers(SupplierId);

CREATE INDEX IX_ProjectActivities_ProjectId_CreatedAt ON ProjectActivities(ProjectId, CreatedAt DESC);
CREATE INDEX IX_ProjectNotifications_UserId_IsRead ON ProjectNotifications(UserId, IsRead);

-- =============================================
-- Insert Default Workflow Templates
-- =============================================
INSERT INTO WorkflowTemplates (TemplateName, TemplateType, Description)
VALUES 
    ('Standard RFQ Process', 'BuyerRFQ', 'Standard workflow for buyer-initiated RFQs'),
    ('Fast Track RFQ', 'BuyerRFQ', 'Expedited RFQ process for urgent requirements'),
    ('Standard Supplier Offer', 'SupplierOffer', 'Standard workflow for supplier-initiated offers'),
    ('Direct Negotiation', 'DirectNegotiation', 'Direct negotiation between buyer and supplier');

-- Insert stages for Standard RFQ Process
DECLARE @RFQTemplateId INT = (SELECT Id FROM WorkflowTemplates WHERE TemplateName = 'Standard RFQ Process');

INSERT INTO WorkflowStages (TemplateId, StageName, StageOrder, Description, DefaultDuration, IsMandatory, RequiresApproval, DefaultAssigneeRole)
VALUES 
    (@RFQTemplateId, 'Draft', 1, 'Prepare RFQ requirements', 2, 1, 0, 'Buyer'),
    (@RFQTemplateId, 'Internal Review', 2, 'Internal review and approval', 1, 1, 1, 'Manager'),
    (@RFQTemplateId, 'Published', 3, 'RFQ published to suppliers', 0, 1, 0, 'System'),
    (@RFQTemplateId, 'Quoting', 4, 'Suppliers submit quotes', 7, 1, 0, 'Supplier'),
    (@RFQTemplateId, 'Technical Evaluation', 5, 'Technical evaluation of quotes', 3, 1, 0, 'Expert'),
    (@RFQTemplateId, 'Commercial Evaluation', 6, 'Commercial evaluation of quotes', 2, 1, 0, 'Procurement'),
    (@RFQTemplateId, 'Negotiation', 7, 'Price and terms negotiation', 5, 0, 0, 'Procurement'),
    (@RFQTemplateId, 'Award Approval', 8, 'Final approval for award', 1, 1, 1, 'Manager'),
    (@RFQTemplateId, 'Contracting', 9, 'Contract preparation and signing', 3, 1, 0, 'Legal'),
    (@RFQTemplateId, 'Execution', 10, 'Order fulfillment and delivery', 30, 1, 0, 'Operations'),
    (@RFQTemplateId, 'Completed', 11, 'Project closure', 1, 1, 0, 'System');

PRINT 'Project Management tables created successfully!';
GO
-- FoodX.Simple Workflow Testing Script
-- This script tests the complete workflow: ProductBrief -> RFQ -> Project

-- First, let's check current database state
SELECT 'Current Database State:' as Step;
SELECT 'ProductBriefs' as TableName, COUNT(*) as RecordCount FROM ProductBriefs;
SELECT 'RFQs' as TableName, COUNT(*) as RecordCount FROM RFQs;
SELECT 'Projects' as TableName, COUNT(*) as RecordCount FROM Projects;

-- Step 1: Insert a realistic Product Brief
DECLARE @BriefId INT;

INSERT INTO ProductBriefs (
    ProductName,
    Category,
    BenchmarkBrandReference,
    PackageSize,
    StorageRequirements,
    CountryOfOrigin,
    IsKosherCertified,
    KosherOrganization,
    SpecialAttributes,
    AdditionalNotes,
    CreatedDate,
    CreatedBy,
    Status
) VALUES (
    'Premium Chocolate Sandwich Cookies',
    'Biscuits & Cookies',
    'Oreo Cookies',
    '200g',
    'Ambient',
    'Poland',
    1,
    'OU',
    'Gluten Free, Non-GMO',
    'Kosher Requirements:
- Organization: OU
- Status: Pareve
- Kosher for Passover Required

Certifications: HACCP, Organic
Packaging: Retail Pack',
    GETUTCDATE(),
    'TestUser',
    'Active'
);

SET @BriefId = SCOPE_IDENTITY();
SELECT 'ProductBrief Created:' as Step, @BriefId as BriefId;

-- Step 2: Simulate RFQ Creation (what the AutomaticWorkflowService should do)
DECLARE @RFQId INT;
DECLARE @RFQNumber NVARCHAR(100);

-- Generate RFQ Number
DECLARE @CurrentYear INT = YEAR(GETUTCDATE());
DECLARE @RFQCount INT;
SELECT @RFQCount = ISNULL(COUNT(*), 0) FROM RFQs WHERE RFQNumber LIKE 'RFQ-' + CAST(@CurrentYear AS NVARCHAR) + '-%';
SET @RFQNumber = 'RFQ-' + CAST(@CurrentYear AS NVARCHAR) + '-' + FORMAT(@RFQCount + 1, '000');

INSERT INTO RFQs (
    RFQNumber,
    Title,
    Description,
    Category,
    PackageSize,
    CountryOfOrigin,
    IsKosherCertified,
    KosherOrganization,
    SpecialAttributes,
    AdditionalNotes,
    IssueDate,
    ResponseDeadline,
    ProductBriefId,
    CreatedBy,
    Status
)
SELECT
    @RFQNumber,
    ProductName,
    ISNULL(AdditionalNotes, 'RFQ for ' + ProductName),
    Category,
    PackageSize,
    CountryOfOrigin,
    IsKosherCertified,
    KosherOrganization,
    SpecialAttributes,
    AdditionalNotes,
    GETUTCDATE(),
    DATEADD(DAY, 14, GETUTCDATE()),
    Id,
    CreatedBy,
    'Active'
FROM ProductBriefs
WHERE Id = @BriefId;

SET @RFQId = SCOPE_IDENTITY();
SELECT 'RFQ Created:' as Step, @RFQId as RFQId, @RFQNumber as RFQNumber;

-- Step 3: Simulate Project Creation (what the AutomaticWorkflowService should do)
DECLARE @ProjectId INT;
DECLARE @ProjectNumber NVARCHAR(100);

-- Generate Project Number
DECLARE @ProjectCount INT;
SELECT @ProjectCount = ISNULL(COUNT(*), 0) FROM Projects WHERE ProjectNumber LIKE 'PRJ-' + CAST(@CurrentYear AS NVARCHAR) + '-%';
SET @ProjectNumber = 'PRJ-' + CAST(@CurrentYear AS NVARCHAR) + '-' + FORMAT(@ProjectCount + 1, '000');

INSERT INTO Projects (
    ProjectNumber,
    Title,
    Description,
    Status,
    Priority,
    StartDate,
    ExpectedEndDate,
    AssignedTo,
    RFQId,
    CreatedBy,
    Notes
)
SELECT
    @ProjectNumber,
    'Project: ' + r.Title,
    'Procurement project for ' + r.Title,
    'Planning',
    'Medium',
    GETUTCDATE(),
    DATEADD(DAY, 30, r.ResponseDeadline),
    r.CreatedBy,
    r.Id,
    r.CreatedBy,
    'Auto-generated project for RFQ ' + r.RFQNumber
FROM RFQs r
WHERE r.Id = @RFQId;

SET @ProjectId = SCOPE_IDENTITY();
SELECT 'Project Created:' as Step, @ProjectId as ProjectId, @ProjectNumber as ProjectNumber;

-- Step 4: Verify the complete workflow
SELECT 'Workflow Verification:' as Step;

SELECT
    pb.Id as BriefId,
    pb.ProductName,
    pb.Status as BriefStatus,
    r.Id as RFQId,
    r.RFQNumber,
    r.Status as RFQStatus,
    p.Id as ProjectId,
    p.ProjectNumber,
    p.Status as ProjectStatus
FROM ProductBriefs pb
LEFT JOIN RFQs r ON pb.Id = r.ProductBriefId
LEFT JOIN Projects p ON r.Id = p.RFQId
WHERE pb.Id = @BriefId;

-- Step 5: Check for any orphaned records
SELECT 'Data Integrity Check:' as Step;

SELECT 'Briefs without RFQ:' as Issue, COUNT(*) as Count
FROM ProductBriefs pb
WHERE pb.Status = 'Active'
  AND NOT EXISTS (SELECT 1 FROM RFQs r WHERE r.ProductBriefId = pb.Id);

SELECT 'RFQs without Project:' as Issue, COUNT(*) as Count
FROM RFQs r
WHERE NOT EXISTS (SELECT 1 FROM Projects p WHERE p.RFQId = r.Id);

-- Step 6: Show final database state
SELECT 'Final Database State:' as Step;
SELECT 'ProductBriefs' as TableName, COUNT(*) as RecordCount FROM ProductBriefs;
SELECT 'RFQs' as TableName, COUNT(*) as RecordCount FROM RFQs;
SELECT 'Projects' as TableName, COUNT(*) as RecordCount FROM Projects;

-- Step 7: Test search functionality simulation
SELECT 'Search Functionality Test:' as Step;

-- Simulate searching for briefs (what the UI would do)
SELECT
    Id,
    ProductName,
    Category,
    Status,
    CreatedDate,
    CreatedBy
FROM ProductBriefs
WHERE Status = 'Active'
ORDER BY CreatedDate DESC;

-- Simulate searching for RFQs (what the RFQs page would do)
SELECT
    Id,
    RFQNumber,
    Title,
    Category,
    IssueDate,
    ResponseDeadline,
    Status
FROM RFQs
ORDER BY IssueDate DESC;

SELECT 'Workflow Test Complete' as Result;
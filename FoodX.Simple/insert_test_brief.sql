-- Insert test ProductBrief record to verify automatic workflow
-- This will allow us to test if the workflow is working correctly

INSERT INTO ProductBriefs (
    ProductName,
    Category,
    PackageSize,
    CountryOfOrigin,
    IsKosherCertified,
    KosherOrganization,
    AdditionalNotes,
    CreatedDate,
    CreatedBy,
    Status,
    IsWorkflowCompleted,
    WorkflowCompletedDate
) VALUES (
    'Organic Quinoa',
    'Grains',
    '25kg bags',
    'Peru',
    1, -- true for IsKosherCertified
    'OU',
    'Premium quality organic quinoa for test workflow. Kosher Requirements: Organization: OU',
    GETUTCDATE(),
    'test-user',
    'Draft', -- Set as Draft initially, will be updated to Active by workflow
    0, -- false for IsWorkflowCompleted initially
    NULL -- WorkflowCompletedDate is null initially
);

-- Check if the record was inserted
SELECT TOP 1 * FROM ProductBriefs ORDER BY CreatedDate DESC;
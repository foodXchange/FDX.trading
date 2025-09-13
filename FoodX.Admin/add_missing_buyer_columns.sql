-- Add missing columns to FoodXBuyers table
ALTER TABLE FoodXBuyers
ADD BuyerId NVARCHAR(100) NULL,
    AboutCompany NVARCHAR(MAX) NULL,
    AiReady BIT NULL DEFAULT 0,
    AnnualRevenue DECIMAL(18,2) NULL,
    BusinessTypeScore INT NULL,
    DataCompletenessScore INT NULL,
    EmployeeCount INT NULL,
    EngagementReadinessScore INT NULL,
    FoodFocus NVARCHAR(500) NULL,
    FoundedYear INT NULL,
    HqCity NVARCHAR(100) NULL,
    PriceSensitivityIndex DECIMAL(5,2) NULL,
    PrimaryContactEmail NVARCHAR(256) NULL,
    PrimaryContactName NVARCHAR(200) NULL,
    PrimaryContactTitle NVARCHAR(100) NULL,
    ProductCategories NVARCHAR(MAX) NULL,
    PurchasingPowerScore INT NULL,
    StoreCount INT NULL,
    VerificationStatus NVARCHAR(50) NULL;

-- Add indexes for better performance
CREATE INDEX IX_FoodXBuyers_BuyerId ON FoodXBuyers(BuyerId);
CREATE INDEX IX_FoodXBuyers_AiReady ON FoodXBuyers(AiReady);
CREATE INDEX IX_FoodXBuyers_VerificationStatus ON FoodXBuyers(VerificationStatus);
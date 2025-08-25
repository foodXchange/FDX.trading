-- Create BuyerRequests table for storing product search requests
CREATE TABLE BuyerRequests (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    BuyerId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    InputType VARCHAR(20) NOT NULL CHECK (InputType IN ('Image', 'URL', 'Text')),
    InputContent NVARCHAR(MAX) NOT NULL, -- Stores URL, text description, or image path
    Status VARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Processing', 'Analyzed', 'Approved', 'Failed')),
    Notes NVARCHAR(MAX) NULL, -- Buyer's additional notes
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (BuyerId) REFERENCES FoodXBuyers(Id)
);

-- Create AIAnalysisResults table for storing AI-generated product definitions
CREATE TABLE AIAnalysisResults (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RequestId INT NOT NULL,
    AnalysisData NVARCHAR(MAX) NOT NULL, -- JSON containing all AI findings
    ConfidenceScore DECIMAL(5,2) NULL CHECK (ConfidenceScore >= 0 AND ConfidenceScore <= 100),
    ProcessingTime INT NULL, -- Processing time in milliseconds
    AIProvider VARCHAR(50) NULL, -- Which AI service was used
    ProcessedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (RequestId) REFERENCES BuyerRequests(Id) ON DELETE CASCADE
);

-- Create indexes for better performance
CREATE INDEX IX_BuyerRequests_BuyerId ON BuyerRequests(BuyerId);
CREATE INDEX IX_BuyerRequests_Status ON BuyerRequests(Status);
CREATE INDEX IX_BuyerRequests_CreatedAt ON BuyerRequests(CreatedAt DESC);
CREATE INDEX IX_AIAnalysisResults_RequestId ON AIAnalysisResults(RequestId);

-- Add sample data for testing
INSERT INTO BuyerRequests (BuyerId, Title, InputType, InputContent, Status)
VALUES 
(1, 'Chocolate sandwich cookies request', 'Text', 'I want a product like Oreo cookies', 'Pending'),
(1, 'Pasta sauce inquiry', 'URL', 'https://www.example.com/marinara-sauce', 'Pending'),
(2, 'Olive oil specification', 'Text', 'Looking for extra virgin olive oil similar to Bertolli', 'Pending');

PRINT 'AI Request tables created successfully';
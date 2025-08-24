-- Alternative Vector Store Implementation for Azure SQL Database
-- Since native VECTOR type is not available, we'll create a custom solution

-- Drop existing tables if they exist
IF OBJECT_ID('VectorStore', 'U') IS NOT NULL DROP TABLE VectorStore;
IF OBJECT_ID('VectorDimensions', 'U') IS NOT NULL DROP TABLE VectorDimensions;
GO

-- Create main vector store table
CREATE TABLE VectorStore (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EntityType NVARCHAR(50) NOT NULL, -- 'Product', 'Company', 'User', etc.
    EntityId INT NOT NULL,
    VectorType NVARCHAR(50) NOT NULL, -- 'name', 'description', 'combined'
    VectorData NVARCHAR(MAX) NOT NULL, -- JSON array of float values
    Dimensions INT NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    
    -- Create index for lookups
    INDEX IX_VectorStore_Entity NONCLUSTERED (EntityType, EntityId),
    INDEX IX_VectorStore_Type NONCLUSTERED (VectorType)
);
GO

-- Create a table to store individual vector dimensions for efficient similarity search
CREATE TABLE VectorDimensions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    VectorStoreId INT NOT NULL FOREIGN KEY REFERENCES VectorStore(Id) ON DELETE CASCADE,
    DimensionIndex INT NOT NULL,
    Value FLOAT NOT NULL,
    
    -- Create clustered index for efficient similarity calculations
    INDEX IX_VectorDimensions_Lookup NONCLUSTERED (VectorStoreId, DimensionIndex) INCLUDE (Value)
);
GO

-- Function to calculate cosine similarity between two vectors
CREATE OR ALTER FUNCTION fn_CosineSimilarity
(
    @Vector1Id INT,
    @Vector2Id INT
)
RETURNS FLOAT
AS
BEGIN
    DECLARE @Similarity FLOAT;
    
    -- Calculate cosine similarity: (A·B) / (||A|| * ||B||)
    WITH VectorCalc AS (
        SELECT 
            SUM(v1.Value * v2.Value) AS DotProduct,
            SQRT(SUM(v1.Value * v1.Value)) AS Norm1,
            SQRT(SUM(v2.Value * v2.Value)) AS Norm2
        FROM VectorDimensions v1
        INNER JOIN VectorDimensions v2 
            ON v1.DimensionIndex = v2.DimensionIndex
        WHERE v1.VectorStoreId = @Vector1Id
            AND v2.VectorStoreId = @Vector2Id
    )
    SELECT @Similarity = 
        CASE 
            WHEN Norm1 * Norm2 = 0 THEN 0
            ELSE DotProduct / (Norm1 * Norm2)
        END
    FROM VectorCalc;
    
    RETURN ISNULL(@Similarity, 0);
END
GO

-- Stored procedure to insert a vector
CREATE OR ALTER PROCEDURE sp_InsertVector
    @EntityType NVARCHAR(50),
    @EntityId INT,
    @VectorType NVARCHAR(50),
    @VectorData NVARCHAR(MAX), -- JSON array like '[0.1, 0.2, 0.3, ...]'
    @Dimensions INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Delete existing vector if it exists
        DELETE vd
        FROM VectorDimensions vd
        INNER JOIN VectorStore vs ON vd.VectorStoreId = vs.Id
        WHERE vs.EntityType = @EntityType 
            AND vs.EntityId = @EntityId
            AND vs.VectorType = @VectorType;
        
        DELETE FROM VectorStore
        WHERE EntityType = @EntityType 
            AND EntityId = @EntityId
            AND VectorType = @VectorType;
        
        -- Insert new vector
        DECLARE @VectorStoreId INT;
        
        INSERT INTO VectorStore (EntityType, EntityId, VectorType, VectorData, Dimensions)
        VALUES (@EntityType, @EntityId, @VectorType, @VectorData, @Dimensions);
        
        SET @VectorStoreId = SCOPE_IDENTITY();
        
        -- Parse JSON and insert dimensions
        INSERT INTO VectorDimensions (VectorStoreId, DimensionIndex, Value)
        SELECT 
            @VectorStoreId,
            [key] AS DimensionIndex,
            CAST([value] AS FLOAT) AS Value
        FROM OPENJSON(@VectorData)
        WHERE ISNUMERIC([value]) = 1;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Stored procedure for vector similarity search
CREATE OR ALTER PROCEDURE sp_VectorSearch
    @SearchVector NVARCHAR(MAX), -- JSON array of the search vector
    @EntityType NVARCHAR(50),
    @VectorType NVARCHAR(50),
    @TopN INT = 10,
    @MinSimilarity FLOAT = 0.5
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Create temporary table for search vector
    CREATE TABLE #SearchVector (
        DimensionIndex INT,
        Value FLOAT
    );
    
    -- Parse search vector
    INSERT INTO #SearchVector (DimensionIndex, Value)
    SELECT 
        [key] AS DimensionIndex,
        CAST([value] AS FLOAT) AS Value
    FROM OPENJSON(@SearchVector)
    WHERE ISNUMERIC([value]) = 1;
    
    -- Calculate similarities
    WITH Similarities AS (
        SELECT 
            vs.EntityId,
            vs.Id AS VectorStoreId,
            -- Cosine similarity calculation
            SUM(vd.Value * sv.Value) / 
            (SQRT(SUM(vd.Value * vd.Value)) * SQRT(SUM(sv.Value * sv.Value))) AS Similarity
        FROM VectorStore vs
        INNER JOIN VectorDimensions vd ON vs.Id = vd.VectorStoreId
        INNER JOIN #SearchVector sv ON vd.DimensionIndex = sv.DimensionIndex
        WHERE vs.EntityType = @EntityType
            AND vs.VectorType = @VectorType
        GROUP BY vs.EntityId, vs.Id
        HAVING SUM(vd.Value * sv.Value) / 
               (SQRT(SUM(vd.Value * vd.Value)) * SQRT(SUM(sv.Value * sv.Value))) >= @MinSimilarity
    )
    SELECT TOP(@TopN)
        s.EntityId,
        s.Similarity,
        CASE @EntityType
            WHEN 'Product' THEN p.Name
            WHEN 'Company' THEN c.Name
            ELSE NULL
        END AS EntityName,
        CASE @EntityType
            WHEN 'Product' THEN p.Description
            WHEN 'Company' THEN c.Description
            ELSE NULL
        END AS EntityDescription
    FROM Similarities s
    LEFT JOIN Products p ON @EntityType = 'Product' AND s.EntityId = p.Id
    LEFT JOIN Companies c ON @EntityType = 'Company' AND s.EntityId = c.Id
    ORDER BY s.Similarity DESC;
    
    DROP TABLE #SearchVector;
END
GO

-- Create a metadata table for tracking embedding models and configurations
CREATE TABLE VectorMetadata (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ModelName NVARCHAR(100) NOT NULL, -- e.g., 'text-embedding-ada-002', 'text-embedding-3-small'
    ModelVersion NVARCHAR(50),
    Dimensions INT NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    Configuration NVARCHAR(MAX) -- JSON for additional settings
);
GO

-- Insert default embedding model configuration
INSERT INTO VectorMetadata (ModelName, ModelVersion, Dimensions, Configuration)
VALUES 
    ('text-embedding-ada-002', 'v1', 1536, '{"provider": "OpenAI", "maxTokens": 8191}'),
    ('text-embedding-3-small', 'v1', 1536, '{"provider": "OpenAI", "maxTokens": 8191}'),
    ('text-embedding-3-large', 'v1', 3072, '{"provider": "OpenAI", "maxTokens": 8191, "dimensions": 1024}');
GO

-- Stored procedure to find similar products
CREATE OR ALTER PROCEDURE sp_FindSimilarProducts
    @ProductId INT,
    @TopN INT = 5
AS
BEGIN
    DECLARE @VectorStoreId INT;
    
    -- Get the vector store ID for this product
    SELECT TOP 1 @VectorStoreId = Id
    FROM VectorStore
    WHERE EntityType = 'Product' 
        AND EntityId = @ProductId
        AND VectorType = 'combined';
    
    IF @VectorStoreId IS NOT NULL
    BEGIN
        WITH Similarities AS (
            SELECT 
                vs2.EntityId,
                dbo.fn_CosineSimilarity(@VectorStoreId, vs2.Id) AS Similarity
            FROM VectorStore vs2
            WHERE vs2.EntityType = 'Product'
                AND vs2.VectorType = 'combined'
                AND vs2.EntityId != @ProductId
        )
        SELECT TOP(@TopN)
            p.Id,
            p.Name,
            p.Category,
            p.Description,
            p.Price,
            s.Similarity
        FROM Similarities s
        INNER JOIN Products p ON s.EntityId = p.Id
        WHERE p.IsActive = 1
        ORDER BY s.Similarity DESC;
    END
END
GO

-- Create search history table for analytics
CREATE TABLE SearchHistory (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId NVARCHAR(450),
    SearchQuery NVARCHAR(500),
    SearchType NVARCHAR(50),
    ResultCount INT,
    TopResultId INT,
    TopResultSimilarity FLOAT,
    ExecutionTimeMs INT,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
GO

PRINT 'Vector store tables and procedures created successfully!';
PRINT 'This implementation provides:';
PRINT '1. VectorStore table for storing embeddings';
PRINT '2. VectorDimensions table for efficient similarity calculations';
PRINT '3. Stored procedures for vector search and similarity';
PRINT '4. Support for multiple entity types (Products, Companies, etc.)';
PRINT '5. Configurable embedding models via VectorMetadata';
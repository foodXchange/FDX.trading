-- Enable Vector Support in Azure SQL Database
-- Vector support is in Public Preview as of November 2024

-- Test if vector data type is available
BEGIN TRY
    DECLARE @test_vector VECTOR(3) = '[1.0, 2.0, 3.0]';
    PRINT 'Vector support is available!';
END TRY
BEGIN CATCH
    PRINT 'Vector support is not available. Error: ' + ERROR_MESSAGE();
END CATCH
GO

-- Add vector columns to Products table for semantic search
-- We'll store embeddings for product names and descriptions
ALTER TABLE Products
ADD NameEmbedding VECTOR(1536) NULL,
    DescriptionEmbedding VECTOR(1536) NULL;
GO

-- Add vector columns to Companies table
ALTER TABLE Companies  
ADD CompanyEmbedding VECTOR(1536) NULL;
GO

-- Create a table to store search queries and their embeddings (for analytics)
CREATE TABLE SearchQueries (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Query NVARCHAR(500) NOT NULL,
    QueryEmbedding VECTOR(1536) NULL,
    UserId NVARCHAR(450) NULL,
    SearchType NVARCHAR(50) NOT NULL, -- 'Product', 'Company', 'Combined'
    ResultCount INT,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
GO

-- Create stored procedure for vector similarity search on Products
CREATE OR ALTER PROCEDURE sp_SearchProductsByVector
    @QueryEmbedding VECTOR(1536),
    @TopN INT = 10,
    @MinSimilarity FLOAT = 0.7
AS
BEGIN
    SELECT TOP(@TopN)
        p.Id,
        p.Name,
        p.Category,
        p.Description,
        p.Price,
        p.SKU,
        p.Origin,
        p.IsOrganic,
        VECTOR_DISTANCE('cosine', @QueryEmbedding, p.NameEmbedding) AS NameSimilarity,
        VECTOR_DISTANCE('cosine', @QueryEmbedding, p.DescriptionEmbedding) AS DescriptionSimilarity,
        -- Combined score (weighted average)
        (0.6 * VECTOR_DISTANCE('cosine', @QueryEmbedding, p.NameEmbedding) + 
         0.4 * VECTOR_DISTANCE('cosine', @QueryEmbedding, p.DescriptionEmbedding)) AS CombinedSimilarity
    FROM Products p
    WHERE p.IsActive = 1
        AND p.NameEmbedding IS NOT NULL
        AND VECTOR_DISTANCE('cosine', @QueryEmbedding, p.NameEmbedding) >= @MinSimilarity
    ORDER BY CombinedSimilarity DESC;
END
GO

-- Create stored procedure for hybrid search (vector + traditional)
CREATE OR ALTER PROCEDURE sp_HybridProductSearch
    @QueryText NVARCHAR(500),
    @QueryEmbedding VECTOR(1536) = NULL,
    @TopN INT = 10
AS
BEGIN
    -- Combine vector similarity with traditional text search
    WITH VectorResults AS (
        SELECT 
            p.Id,
            p.Name,
            p.Category,
            p.Description,
            p.Price,
            CASE 
                WHEN @QueryEmbedding IS NOT NULL THEN
                    VECTOR_DISTANCE('cosine', @QueryEmbedding, p.NameEmbedding)
                ELSE 0
            END AS VectorScore
        FROM Products p
        WHERE p.IsActive = 1
            AND (@QueryEmbedding IS NULL OR p.NameEmbedding IS NOT NULL)
    ),
    TextResults AS (
        SELECT 
            p.Id,
            CASE
                WHEN p.Name LIKE '%' + @QueryText + '%' THEN 1.0
                WHEN p.Description LIKE '%' + @QueryText + '%' THEN 0.7
                WHEN p.Category LIKE '%' + @QueryText + '%' THEN 0.5
                ELSE 0
            END AS TextScore
        FROM Products p
        WHERE p.IsActive = 1
            AND (@QueryText IS NULL OR 
                 p.Name LIKE '%' + @QueryText + '%' OR
                 p.Description LIKE '%' + @QueryText + '%' OR
                 p.Category LIKE '%' + @QueryText + '%')
    )
    SELECT TOP(@TopN)
        v.Id,
        v.Name,
        v.Category,
        v.Description,
        v.Price,
        v.VectorScore,
        ISNULL(t.TextScore, 0) AS TextScore,
        -- Combined score: 70% vector, 30% text
        (0.7 * v.VectorScore + 0.3 * ISNULL(t.TextScore, 0)) AS CombinedScore
    FROM VectorResults v
    LEFT JOIN TextResults t ON v.Id = t.Id
    WHERE v.VectorScore > 0 OR t.TextScore > 0
    ORDER BY CombinedScore DESC;
END
GO

-- Create stored procedure for finding similar products
CREATE OR ALTER PROCEDURE sp_FindSimilarProducts
    @ProductId INT,
    @TopN INT = 5
AS
BEGIN
    DECLARE @ProductEmbedding VECTOR(1536);
    
    -- Get the embedding of the source product
    SELECT @ProductEmbedding = NameEmbedding
    FROM Products
    WHERE Id = @ProductId;
    
    IF @ProductEmbedding IS NOT NULL
    BEGIN
        SELECT TOP(@TopN)
            p.Id,
            p.Name,
            p.Category,
            p.Description,
            p.Price,
            VECTOR_DISTANCE('cosine', @ProductEmbedding, p.NameEmbedding) AS Similarity
        FROM Products p
        WHERE p.Id != @ProductId
            AND p.IsActive = 1
            AND p.NameEmbedding IS NOT NULL
        ORDER BY Similarity DESC;
    END
END
GO

-- Create index on vector columns for better performance
-- Note: Column store indexes can significantly improve vector search performance
CREATE COLUMNSTORE INDEX IX_Products_Vectors
ON Products (Id, Name, Category, NameEmbedding, DescriptionEmbedding)
WHERE NameEmbedding IS NOT NULL;
GO

CREATE COLUMNSTORE INDEX IX_Companies_Vectors
ON Companies (Id, Name, CompanyType, CompanyEmbedding)
WHERE CompanyEmbedding IS NOT NULL;
GO

PRINT 'Vector support setup completed successfully!';
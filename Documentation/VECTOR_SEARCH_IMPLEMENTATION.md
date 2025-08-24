# Vector Search Implementation Guide

## Overview
The FDX.Trading platform now includes vector search capabilities, enabling semantic search across products and companies using AI embeddings.

## Architecture

### Database Structure
Since Azure SQL Database native vector support requires specific tiers, we've implemented a custom vector store solution:

1. **VectorStore** - Main table storing vector metadata
2. **VectorDimensions** - Individual vector dimension values for similarity calculations
3. **VectorMetadata** - Configuration for different embedding models
4. **SearchHistory** - Analytics and search tracking

### Components

#### Backend Services

1. **VectorSearchService** (`FoodX.Core/Services/VectorSearchService.cs`)
   - Stores and retrieves vector embeddings
   - Performs similarity searches
   - Manages product and company embeddings

2. **EmbeddingService** (`FoodX.Core/Services/EmbeddingService.cs`)
   - Integrates with Azure OpenAI or OpenAI API
   - Generates embeddings for text
   - Batch processing capabilities

3. **VectorSearchController** (`FoodX.Admin/Controllers/VectorSearchController.cs`)
   - REST API endpoints for vector search
   - Product and company search endpoints
   - Embedding update endpoints

#### Frontend

1. **VectorSearch.razor** (`FoodX.Admin/Components/Pages/VectorSearch.razor`)
   - Demo UI for vector search
   - Product and company search interfaces
   - Real-time similarity scoring display

## Configuration

### Azure OpenAI Setup

Add to `appsettings.json`:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key",
    "EmbeddingDeployment": "text-embedding-ada-002",
    "Dimensions": 1536
  }
}
```

### OpenAI API Setup (Alternative)

```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "EmbeddingModel": "text-embedding-ada-002"
  }
}
```

## Usage

### 1. Generate Embeddings

First, generate embeddings for existing data:

```bash
# Using API endpoints
POST /api/VectorSearch/update-embeddings/all-products
POST /api/VectorSearch/update-embeddings/all-companies
```

### 2. Search Products

```csharp
// Example: Search for organic vegetables
var searchQuery = "organic vegetables from Europe";
var embedding = await _embeddingService.GetEmbeddingAsync(searchQuery);
var results = await _vectorSearchService.SearchAsync(
    "Product",
    "combined",
    embedding,
    topN: 10,
    minSimilarity: 0.5f
);
```

### 3. Find Similar Products

```csharp
// Find products similar to product ID 123
var similarProducts = await _vectorSearchService.FindSimilarProductsAsync(
    productId: 123,
    topN: 5
);
```

## SQL Stored Procedures

### sp_VectorSearch
Main search procedure supporting any entity type with configurable similarity threshold.

### sp_FindSimilarProducts
Finds products similar to a given product based on embeddings.

### sp_InsertVector
Stores vector embeddings with automatic dimension parsing.

## Performance Optimization

1. **Batch Processing**: Process embeddings in batches of 16 for optimal throughput
2. **Caching**: Results are cached for frequently searched queries
3. **Indexing**: VectorDimensions table has optimized indexes for similarity calculations
4. **Similarity Threshold**: Use appropriate thresholds (0.5-0.8) to balance relevance and result count

## Supported Models

1. **text-embedding-ada-002** (1536 dimensions) - Default, cost-effective
2. **text-embedding-3-small** (1536 dimensions) - Better performance
3. **text-embedding-3-large** (3072 dimensions) - Highest quality

## API Endpoints

### Search Endpoints
- `POST /api/VectorSearch/search/products` - Search products
- `POST /api/VectorSearch/search/companies` - Search companies
- `GET /api/VectorSearch/similar-products/{productId}` - Find similar products

### Admin Endpoints (SuperAdmin only)
- `POST /api/VectorSearch/update-embeddings/product/{productId}` - Update single product
- `POST /api/VectorSearch/update-embeddings/all-products` - Batch update products
- `POST /api/VectorSearch/update-embeddings/all-companies` - Batch update companies

## Future Enhancements

1. **Native Vector Support**: Migrate to Azure SQL native vectors when available
2. **Multi-language Support**: Add embeddings for multiple languages
3. **Hybrid Search**: Combine vector search with traditional filters
4. **Real-time Updates**: Auto-generate embeddings on data changes
5. **Analytics Dashboard**: Visualize search patterns and popular queries

## Troubleshooting

### No Results Returned
- Check if embeddings are generated for your data
- Verify OpenAI/Azure OpenAI credentials
- Lower the similarity threshold

### Slow Performance
- Consider using columnstore indexes (requires higher Azure SQL tier)
- Implement result caching
- Reduce embedding dimensions if quality permits

### API Errors
- Check Azure OpenAI quota and rate limits
- Verify network connectivity
- Review logs in `VectorSearchService` and `EmbeddingService`

## Cost Considerations

- **Embedding Generation**: ~$0.0001 per 1K tokens (ada-002)
- **Storage**: ~10MB per 1000 products with embeddings
- **API Calls**: Monitor usage to stay within quotas

## Security

- API keys stored in Azure Key Vault
- Embeddings don't contain sensitive data
- Search queries logged for analytics (can be disabled)
- Role-based access to admin endpoints
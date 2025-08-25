# Azure AI Services Integration

## Current Status

### ✅ Implemented Infrastructure

1. **Azure OpenAI Integration**
   - Service: `AzureOpenAIEmbeddingService` in FoodX.Core
   - Supports text-embedding-ada-002 model
   - Batch processing capabilities
   - Already configured for vector search

2. **AI Request Analyzer Service**
   - Location: `FoodX.Admin\Services\AIRequestAnalyzer.cs`
   - Supports both Azure OpenAI and standard OpenAI API
   - Fallback to mock data when no API keys configured
   - Ready for production with API keys

3. **Embedding Service**
   - Existing implementation for product embeddings
   - Used by vector search functionality
   - Can be leveraged for semantic analysis

## Configuration

### Azure OpenAI (Preferred)
Add to `appsettings.json` or Azure Key Vault:
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key",
    "EmbeddingDeployment": "text-embedding-ada-002",
    "ChatDeployment": "gpt-35-turbo"
  }
}
```

### Standard OpenAI (Alternative)
```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "EmbeddingModel": "text-embedding-ada-002",
    "ChatModel": "gpt-3.5-turbo"
  }
}
```

### Azure Computer Vision (Future)
```json
{
  "AzureComputerVision": {
    "Endpoint": "https://your-vision.cognitiveservices.azure.com/",
    "ApiKey": "your-api-key"
  }
}
```

## AI Request Analysis Features

### Current Capabilities
1. **Text Analysis**
   - Natural language product descriptions
   - "I want a product like X" queries
   - Generates comprehensive specifications

2. **URL Analysis**
   - Fetches product pages
   - Extracts product information
   - Converts to structured data

3. **Image Analysis (Ready for Enhancement)**
   - Image upload support
   - Base64 encoding for API calls
   - Placeholder for Computer Vision integration

### AI-Generated Product Specifications Include:
- Product identification and confidence scores
- Detailed descriptions and characteristics
- Technical specifications
- Category classification
- Common attributes (ingredients, usage, certifications)
- Market context and positioning

## Azure Resources Status

### Currently Deployed
- **Resource Group**: fdx-dotnet-rg (Poland Central)
- **Key Vault**: fdx-kv-poland
- **SQL Database**: fdx-sql-prod

### Not Yet Deployed (Needed for Production)
- Azure OpenAI Service
- Azure Computer Vision Service
- Azure Cognitive Search (optional)

## API Integration Points

### 1. AIRequestController
- `/api/AIRequest/analyze-text`
- `/api/AIRequest/analyze-url`
- `/api/AIRequest/analyze-image`

### 2. RequestAnalysis Page
- UI at `/request-analysis`
- Three input methods
- Real-time processing
- Results preview and storage

## Implementation Architecture

```
User Input (Text/URL/Image)
        ↓
AIRequestAnalyzer Service
        ↓
    [Branches]
        ├── Azure OpenAI (if configured)
        ├── Standard OpenAI (if configured)
        └── Mock Data (fallback)
        ↓
ProductAnalysis Object
        ↓
Database Storage (BuyerRequests + AIAnalysisResults)
        ↓
UI Display & Review
```

## Next Steps for Production

### 1. Deploy Azure OpenAI Service
```bash
# Create Azure OpenAI resource
az cognitiveservices account create \
  --name "fdx-openai" \
  --resource-group "fdx-dotnet-rg" \
  --kind "OpenAI" \
  --sku "S0" \
  --location "polandcentral" \
  --yes

# Deploy models
az cognitiveservices account deployment create \
  --name "fdx-openai" \
  --resource-group "fdx-dotnet-rg" \
  --deployment-name "gpt-35-turbo" \
  --model-name "gpt-35-turbo" \
  --model-version "0613" \
  --model-format "OpenAI" \
  --sku-capacity "10" \
  --sku-name "Standard"
```

### 2. Add API Keys to Key Vault
```bash
# Add OpenAI API key
az keyvault secret set \
  --vault-name "fdx-kv-poland" \
  --name "AzureOpenAI--ApiKey" \
  --value "your-api-key"

# Add endpoint
az keyvault secret set \
  --vault-name "fdx-kv-poland" \
  --name "AzureOpenAI--Endpoint" \
  --value "https://fdx-openai.openai.azure.com/"
```

### 3. Deploy Computer Vision (Optional)
```bash
# Create Computer Vision resource
az cognitiveservices account create \
  --name "fdx-vision" \
  --resource-group "fdx-dotnet-rg" \
  --kind "ComputerVision" \
  --sku "F0" \
  --location "polandcentral" \
  --yes
```

## Cost Estimates

### Azure OpenAI
- GPT-3.5 Turbo: ~$0.0015 per 1K tokens
- Embeddings: ~$0.0001 per 1K tokens
- Estimated: $10-50/month for moderate usage

### Computer Vision
- F0 tier: 5,000 transactions free/month
- S1 tier: $1 per 1,000 transactions

## Security Considerations

1. **API Keys**: Store in Azure Key Vault only
2. **Rate Limiting**: Implement throttling for API calls
3. **Input Validation**: Sanitize all user inputs
4. **Data Privacy**: Don't send sensitive data to AI services
5. **Audit Logging**: Track all AI service usage

## Testing Without API Keys

The system currently returns realistic mock data when no API keys are configured:
- Mock Oreo cookie analysis for testing
- Demonstrates full data structure
- Allows UI/UX testing without costs

## Performance Optimizations

1. **Caching**: Cache frequent AI responses
2. **Batch Processing**: Process multiple requests together
3. **Async Operations**: All AI calls are async
4. **Timeout Handling**: 30-second timeout for AI calls
5. **Retry Logic**: Automatic retry on transient failures

## Monitoring & Logging

- Application Insights integration ready
- Detailed logging in AIRequestAnalyzer
- Performance metrics tracking
- Error handling and fallbacks

---

*Last Updated: August 25, 2025*
*Status: Ready for API key configuration*
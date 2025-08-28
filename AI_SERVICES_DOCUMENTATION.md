# Azure OpenAI Services Documentation
## FoodX Platform AI Integration

---

## Overview
The FoodX platform uses Azure OpenAI services for intelligent product search and analysis capabilities. This document outlines the configuration, deployment, and usage of AI services.

---

## Azure Resources

### 1. Azure OpenAI Service
- **Resource Name**: `fdx-openai-poland`
- **Resource Group**: `fdx-dotnet-rg`
- **Location**: Poland Central (same as VM for optimal performance)
- **Tier**: S0 (Standard)
- **Endpoint**: `https://polandcentral.api.cognitive.microsoft.com/`
- **Created**: August 27, 2025

### 2. Deployed Model
- **Model**: GPT-4o
- **Version**: 2024-11-20
- **Deployment Name**: `gpt-4o`
- **SKU**: GlobalStandard
- **Capacity**: 10 units
- **Capabilities**:
  - Chat Completion
  - Vision (Image Analysis)
  - JSON Schema Response
  - Assistants API
  - Real-time Responses

### 3. Rate Limits
- **Requests**: 10 requests per 10 seconds
- **Tokens**: 10,000 tokens per minute

---

## Security Configuration

### Key Vault Storage
All sensitive credentials are stored in Azure Key Vault:

| Secret Name | Description | Key Vault |
|------------|-------------|-----------|
| `AzureOpenAI-ApiKey` | API key for Azure OpenAI | fdx-kv-poland |
| `AzureOpenAI-Endpoint` | Azure OpenAI endpoint URL | fdx-kv-poland |

### Access Configuration
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://polandcentral.api.cognitive.microsoft.com/",
    "ApiKey": "[Loaded from Key Vault]",
    "DeploymentName": "gpt-4o",
    "EmbeddingDeployment": "text-embedding-ada-002",
    "Dimensions": 1536
  }
}
```

---

## Application Integration

### 1. AI Request Analyzer Service
**Location**: `/FoodX.Admin/Services/AIRequestAnalyzer.cs`

**Capabilities**:
- Text-based product analysis
- Image-based product recognition (Vision API)
- URL content analysis
- Product attribute extraction

### 2. Key Features

#### Text Analysis
- Natural language product search
- Requirement extraction from descriptions
- Supplier matching based on text queries

#### Vision Analysis (Image Upload)
- Product package recognition
- Label text extraction
- Certification detection (Kosher, Halal, Organic)
- Nutritional information extraction
- Brand and manufacturer identification

#### URL Analysis
- Product page scraping
- Specification extraction
- Benchmark product analysis

### 3. Service Configuration
```csharp
// Program.cs
builder.Services.AddScoped<IAIRequestAnalyzer, AIRequestAnalyzer>();
builder.Services.AddScoped<IEmbeddingService, AzureOpenAIEmbeddingService>();
```

---

## API Usage Examples

### 1. Text Search
```http
POST /api/ai/analyze-text
Content-Type: application/json

{
  "text": "Looking for organic tomatoes for pizza production"
}
```

### 2. Image Analysis
```http
POST /api/ai/analyze-image
Content-Type: multipart/form-data

[Binary image data]
```

### 3. URL Analysis
```http
POST /api/ai/analyze-url
Content-Type: application/json

{
  "url": "https://example.com/product/organic-tomatoes"
}
```

---

## Cost Management

### Current Setup
- **Single Azure OpenAI instance** (no duplicates)
- **Location**: Poland Central (same as VM - reduces data transfer costs)
- **Tier**: S0 (pay-per-use model)

### Cost Optimization Tips
1. Monitor token usage in Azure Portal
2. Use caching for repeated queries
3. Implement rate limiting in application
4. Set up cost alerts in Azure

### Monitoring
- Azure Portal: Monitor usage and costs
- Application Insights: Track API performance
- Key Vault: Audit credential access

---

## Development & Testing

### Local Development
For local development without Azure access:
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://polandcentral.api.cognitive.microsoft.com/",
    "ApiKey": "[Dev API Key]",
    "DeploymentName": "gpt-4o"
  }
}
```

### Testing Vision API
1. Navigate to: `http://localhost:5195/portal/buyer/ai-search`
2. Upload an image of a food product
3. View extracted information and analysis

### Troubleshooting

#### Issue: "Vision API not configured"
**Solution**: Ensure Azure OpenAI keys are in Key Vault:
```bash
az keyvault secret show --vault-name fdx-kv-poland --name AzureOpenAI-ApiKey
```

#### Issue: Rate limit exceeded
**Solution**: Implement retry logic with exponential backoff

#### Issue: High latency
**Solution**: Verify resource is in Poland Central region

---

## CLI Commands Reference

### Check Azure OpenAI Resource
```bash
# List all Cognitive Services
az cognitiveservices account list --resource-group fdx-dotnet-rg

# Get specific resource details
az cognitiveservices account show --name fdx-openai-poland --resource-group fdx-dotnet-rg

# List deployments
az cognitiveservices account deployment list --name fdx-openai-poland --resource-group fdx-dotnet-rg
```

### Key Vault Management
```bash
# List secrets
az keyvault secret list --vault-name fdx-kv-poland

# Update API key
az keyvault secret set --vault-name fdx-kv-poland --name "AzureOpenAI-ApiKey" --value "[NEW_KEY]"

# Get secret value
az keyvault secret show --vault-name fdx-kv-poland --name "AzureOpenAI-ApiKey"
```

### Model Deployment
```bash
# Deploy new model version
az cognitiveservices account deployment create \
  --name fdx-openai-poland \
  --resource-group fdx-dotnet-rg \
  --deployment-name gpt-4o \
  --model-name gpt-4o \
  --model-version 2024-11-20 \
  --model-format OpenAI \
  --sku-capacity 10 \
  --sku-name GlobalStandard
```

---

## Compliance & Security

### Data Residency
- All AI processing happens in EU (Poland Central)
- No data leaves the EU region
- Compliant with GDPR requirements

### Security Best Practices
1. ✅ Keys stored in Azure Key Vault
2. ✅ Managed Identity access where possible
3. ✅ Network restrictions configured
4. ✅ Audit logging enabled
5. ✅ Regular key rotation

### Access Control
- Only authorized applications can access Key Vault
- Service principals used for production
- Developer access restricted to non-production

---

## Support & Maintenance

### Regular Maintenance Tasks
1. **Monthly**: Review usage metrics and costs
2. **Quarterly**: Rotate API keys
3. **Yearly**: Review model versions and upgrade

### Contact Information
- **Azure Support**: Via Azure Portal
- **Application Issues**: Development team
- **Cost Concerns**: Finance team

### Useful Links
- [Azure OpenAI Documentation](https://docs.microsoft.com/azure/cognitive-services/openai/)
- [GPT-4 Vision Guide](https://platform.openai.com/docs/guides/vision)
- [Azure Key Vault Best Practices](https://docs.microsoft.com/azure/key-vault/general/best-practices)

---

## Version History
| Date | Version | Changes |
|------|---------|---------|
| 2025-08-27 | 1.0 | Initial setup with GPT-4o in Poland Central |

---

*Last Updated: August 27, 2025*
*Document Version: 1.0*
# AI-Powered Product Request Definition System

## Overview
The AI Request Definition System enables buyers to input product requests through multiple methods (text, URL, or image) and leverages AI to comprehensively analyze and define product specifications.

## System Architecture

### Database Tables

#### BuyerRequests
Stores initial product search requests from buyers.
- **Id**: Primary key
- **BuyerId**: Foreign key to FoodXBuyers
- **Title**: Request title/name
- **InputType**: Type of input (Text/URL/Image)
- **InputContent**: The actual input data
- **Status**: Processing status (Pending/Processing/Analyzed/Approved/Failed)
- **Notes**: Additional buyer notes
- **CreatedAt/UpdatedAt**: Timestamps

#### AIAnalysisResults
Stores AI-generated product definitions and specifications.
- **Id**: Primary key
- **RequestId**: Foreign key to BuyerRequests
- **AnalysisData**: JSON containing complete AI analysis
- **ConfidenceScore**: AI confidence level (0-100)
- **ProcessingTime**: Time taken for analysis
- **AIProvider**: Which AI service was used
- **ProcessedAt**: Processing timestamp

## Features

### 1. Multiple Input Methods
- **Text Description**: Buyers can describe products (e.g., "I want cookies like Oreo")
- **Product URL**: Paste links to products from any website
- **Image Upload**: Upload product images for visual analysis

### 2. AI Analysis Components
The system analyzes and extracts:

#### Product Identification
- Detected product name
- Confidence score
- Brand references
- Generic product name

#### Detailed Description
- Product summary
- Key characteristics
- Physical attributes

#### Technical Specifications
- Dimensions
- Composition
- Color profile
- Texture profile

#### Category Classification
- Primary category
- Secondary category
- Specific product type
- Alternative names

#### Common Attributes
- Typical ingredients
- Flavor notes
- Usage occasions
- Shelf life
- Required certifications

#### Market Context
- Common brands
- Typical packaging
- Market positioning
- Price segment

## User Interface

### Request Analysis Page (`/request-analysis`)
Located at: `Components/Pages/RequestAnalysis.razor`

Features:
- Input selection (Text/URL/Image)
- Buyer company selection
- Real-time AI processing
- Results preview and editing
- Recent requests history
- Save and approve functionality

### Navigation
The AI Request Analysis feature is accessible from the main navigation menu under the "Business" section.

## API Endpoints

### AIRequestController
Base route: `/api/AIRequest`

#### Endpoints:
- `GET /api/AIRequest` - Get all buyer requests
- `GET /api/AIRequest/{id}` - Get specific request
- `POST /api/AIRequest/analyze-text` - Analyze text input
- `POST /api/AIRequest/analyze-url` - Analyze URL input
- `POST /api/AIRequest/analyze-image` - Analyze image input
- `PATCH /api/AIRequest/{id}/status` - Update request status
- `DELETE /api/AIRequest/{id}` - Delete request (Admin only)

## Services

### AIRequestAnalyzer Service
Located at: `Services/AIRequestAnalyzer.cs`

Responsibilities:
- Text analysis using OpenAI GPT
- Image analysis (Azure Computer Vision ready)
- URL content extraction and analysis
- Mock data generation for testing

## Configuration

### Required Settings
Add to `appsettings.json`:
```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "Endpoint": "https://api.openai.com/v1"
  }
}
```

For production, store the API key in Azure Key Vault.

## Testing the System

### Sample Test Scenarios

1. **Text Input Test**
   - Navigate to `/request-analysis`
   - Select "Text Description"
   - Enter: "I want a product like Oreo cookies"
   - Click "Analyze with AI"

2. **URL Input Test**
   - Select "Product URL"
   - Enter a product URL
   - Click "Analyze with AI"

3. **Image Upload Test**
   - Select "Upload Image"
   - Upload a product image
   - Click "Analyze with AI"

## Current Implementation Status

### Completed
- ✅ Database tables created
- ✅ Entity models implemented
- ✅ AI service with mock data
- ✅ User interface with all input types
- ✅ API endpoints for integration
- ✅ Navigation integration
- ✅ Basic styling with MudBlazor

### Pending Enhancements
- ⏳ Real OpenAI API integration (currently using mock data)
- ⏳ Azure Computer Vision for image analysis
- ⏳ Advanced web scraping for URL analysis
- ⏳ Supplier matching based on analysis
- ⏳ Bulk request processing
- ⏳ Export functionality for specifications
- ⏳ Request templates and favorites

## Security Considerations

- All endpoints require authentication
- Delete operations restricted to Admin/SuperAdmin roles
- Input validation on all user inputs
- SQL injection prevention through Entity Framework
- XSS protection in Blazor components

## Performance Optimizations

- Async/await pattern throughout
- Database indexes on frequently queried columns
- JSON data storage for flexible schema
- Background processing capability ready
- Caching infrastructure in place

## Future Roadmap

### Phase 2: Enhanced AI Capabilities
- Multi-language support
- Brand detection and comparison
- Nutritional analysis for food products
- Regulatory compliance checking

### Phase 3: Supplier Integration
- Automatic supplier matching
- RFQ generation from specifications
- Price estimation based on specifications
- Historical analysis trends

### Phase 4: Advanced Features
- Machine learning model training on company data
- Custom AI models per product category
- Integration with external product databases
- Automated specification validation

## Troubleshooting

### Common Issues

1. **AI Analysis Returns Mock Data**
   - Check if OpenAI API key is configured
   - Verify network connectivity to OpenAI

2. **Image Upload Fails**
   - Check file size limits
   - Verify supported image formats

3. **Database Errors**
   - Ensure tables are created (run SQL script)
   - Check connection string configuration

## Support

For technical support or feature requests, contact the development team.

---

*Last Updated: August 25, 2025*
*Version: 1.0*
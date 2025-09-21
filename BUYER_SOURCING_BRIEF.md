# Buyer Sourcing Brief Details

## Overview
The Buyer Sourcing Brief (AIRequestBrief) is a comprehensive data model that captures all essential information for procurement requests in the FoodX B2B platform.

## Core Fields

### Basic Information
- **Product Name** (Required): The specific product being sourced
- **Category**: Product category classification
- **Description**: Detailed product description
- **Title**: Brief title for the request

### Quantity & Pricing
- **Quantity**: Required quantity of products
- **Unit**: Unit of measurement (kg, tons, pieces, etc.)
- **Budget Range**:
  - Minimum Budget
  - Maximum Budget

### Delivery Requirements
- **Delivery Date**: Expected delivery timeline
- **Delivery Location**: Shipping/delivery address
- **Payment Terms**: Payment conditions and terms

### Quality & Compliance
- **Quality Requirements**: Specific quality standards required
- **Packaging Requirements**: Packaging specifications
- **Certification Requirements**: Required certifications (ISO, HACCP, Organic, etc.)
- **Specifications**: Technical specifications

### Additional Details
- **Requirements**: General requirements
- **Additional Notes**: Any other relevant information
- **Image URL**: Product image reference

## Status Workflow
- **Draft**: Initial creation
- **Active**: Published for suppliers
- **In Review**: Under evaluation
- **Completed**: Fulfilled

## Integration Points
- **RFQ Generation**: Brief can be converted to formal RFQ
- **Project Association**: Can be linked to larger procurement projects
- **AI Enhancement**: Generated content field for AI-improved descriptions

## Key Features
1. **Comprehensive Data Capture**: All procurement details in one place
2. **Budget Transparency**: Clear budget ranges for suppliers
3. **Quality Assurance**: Detailed quality and certification requirements
4. **Flexible Specifications**: Accommodates various product types
5. **Audit Trail**: Created/Updated timestamps and user tracking

## Usage in Platform

### For Buyers
- Create detailed sourcing requests
- Specify exact requirements
- Set budget expectations
- Define delivery and quality standards

### For Suppliers
- View complete requirement details
- Understand budget constraints
- Assess capability to fulfill
- Prepare accurate quotes

### For Admin
- Monitor sourcing activities
- Analyze procurement patterns
- Facilitate buyer-supplier matching
- Generate insights and reports

## Database Schema
```csharp
public class AIRequestBrief
{
    // Identity
    public int Id { get; set; }
    public string BuyerId { get; set; }

    // Product Details
    public string ProductName { get; set; } // Required
    public string Category { get; set; }
    public string Description { get; set; }

    // Procurement Details
    public int? Quantity { get; set; }
    public string Unit { get; set; }
    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }

    // Delivery
    public DateTime? DeliveryDate { get; set; }
    public string DeliveryLocation { get; set; }

    // Requirements
    public string Requirements { get; set; }
    public string Specifications { get; set; }
    public string QualityRequirements { get; set; }
    public string PackagingRequirements { get; set; }
    public string CertificationRequirements { get; set; }

    // Metadata
    public string Status { get; set; } = "Draft"
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Relationships
    public int? RFQId { get; set; }
    public int? ProjectId { get; set; }
}
```

## Business Value
- **Streamlined Procurement**: Standardized sourcing process
- **Better Matching**: Detailed requirements enable accurate supplier matching
- **Time Savings**: Comprehensive briefs reduce back-and-forth communication
- **Data Analytics**: Structured data enables procurement insights
- **Quality Control**: Clear specifications ensure quality expectations are met
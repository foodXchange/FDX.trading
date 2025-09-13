using System.Text.Json;
using System.Text;
using FoodX.Admin.Models;
using FoodX.Core.Services;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;

namespace FoodX.Admin.Services
{
    public interface IAIRequestAnalyzer
    {
        Task<ProductAnalysis> AnalyzeTextRequest(string text);
        Task<ProductAnalysis> AnalyzeImageRequest(byte[] imageData);
        Task<ProductAnalysis> AnalyzeUrlRequest(string url);
    }

    public class AIRequestAnalyzer : IAIRequestAnalyzer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AIRequestAnalyzer> _logger;
        private readonly HttpClient _httpClient;
        private readonly IEmbeddingService _embeddingService;
        private readonly string _openAiApiKey;
        private readonly string _azureOpenAiEndpoint;
        private readonly string _azureOpenAiKey;
        private readonly bool _useAzureOpenAI;

        public AIRequestAnalyzer(
            IConfiguration configuration,
            ILogger<AIRequestAnalyzer> logger,
            IHttpClientFactory httpClientFactory,
            IEmbeddingService embeddingService)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _embeddingService = embeddingService;

            // Check for Azure OpenAI configuration first (from Key Vault or appsettings)
            _azureOpenAiEndpoint = configuration["AzureOpenAI-Endpoint"] ?? configuration["AzureOpenAI:Endpoint"] ?? "";
            _azureOpenAiKey = configuration["AzureOpenAI-ApiKey"] ?? configuration["AzureOpenAI:ApiKey"] ?? "";
            _useAzureOpenAI = !string.IsNullOrEmpty(_azureOpenAiEndpoint) && !string.IsNullOrEmpty(_azureOpenAiKey);

            // Fall back to OpenAI if Azure OpenAI not configured
            _openAiApiKey = configuration["OpenAI:ApiKey"] ?? "";

            _logger.LogInformation($"AI Request Analyzer initialized with {(_useAzureOpenAI ? "Azure OpenAI" : "OpenAI API")}");
        }

        public async Task<ProductAnalysis> AnalyzeTextRequest(string text)
        {
            try
            {
                _logger.LogInformation("Analyzing text request: {Text}", text);

                // Create the prompt for OpenAI
                var prompt = GenerateTextAnalysisPrompt(text);

                // Call OpenAI API
                var analysisJson = await CallOpenAIApi(prompt);

                // Parse the response
                var analysis = JsonSerializer.Deserialize<ProductAnalysis>(analysisJson) ?? new ProductAnalysis();

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing text request");
                throw;
            }
        }

        public async Task<ProductAnalysis> AnalyzeImageRequest(byte[] imageData)
        {
            try
            {
                _logger.LogInformation("Analyzing image request with Azure OpenAI GPT-4 Vision");

                // Convert image to base64 for GPT-4 Vision
                var base64Image = Convert.ToBase64String(imageData);
                var dataUrl = $"data:image/jpeg;base64,{base64Image}";

                // Use GPT-4 Vision for comprehensive image analysis
                var analysisJson = await CallVisionAPI(dataUrl);

                if (!string.IsNullOrEmpty(analysisJson))
                {
                    // Clean the response before parsing
                    analysisJson = CleanJsonResponse(analysisJson);
                    
                    // Log the cleaned JSON for debugging
                    _logger.LogInformation($"Cleaned JSON response (first 1000 chars): {analysisJson.Substring(0, Math.Min(1000, analysisJson.Length))}");
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    
                    var analysis = JsonSerializer.Deserialize<ProductAnalysis>(analysisJson, options);
                    if (analysis != null)
                    {
                        _logger.LogInformation("Successfully analyzed image with GPT-4 Vision");
                        return analysis;
                    }
                }

                // Fallback to Azure Computer Vision if configured
                var visionEndpoint = _configuration["AzureComputerVision:Endpoint"];
                var visionKey = _configuration["AzureComputerVision:ApiKey"];

                if (!string.IsNullOrEmpty(visionEndpoint) && !string.IsNullOrEmpty(visionKey))
                {
                    return await AnalyzeWithComputerVision(imageData, visionEndpoint, visionKey);
                }

                _logger.LogWarning("Vision analysis failed but returning empty result gracefully");
                
                // Return empty analysis instead of throwing
                return new ProductAnalysis();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing image request - returning empty analysis");
                
                // Return empty analysis instead of throwing
                return new ProductAnalysis();
            }
        }

        private async Task<ProductAnalysis> AnalyzeWithComputerVision(byte[] imageData, string endpoint, string apiKey)
        {
            try
            {
                // For Computer Vision, we'd need the Azure.CognitiveServices.Vision.ComputerVision package
                // For now, we'll use the image data with OpenAI if available

                // Convert image to base64 for potential use with OpenAI Vision
                var base64Image = Convert.ToBase64String(imageData);

                // Generate a description based on image
                var extractedText = "Product image analysis pending Computer Vision setup";

                // Use the extracted text to generate analysis
                var prompt = GenerateImageAnalysisPrompt(extractedText);
                var analysisJson = await CallOpenAIApi(prompt);

                var result = JsonSerializer.Deserialize<ProductAnalysis>(analysisJson);
                if (result == null)
                {
                    throw new InvalidOperationException("Failed to parse AI response.");
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with Computer Vision analysis");
                throw new InvalidOperationException("Computer Vision analysis failed", ex);
            }
        }

        private string CleanJsonResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
                return "{}";
            
            // Remove markdown code blocks
            if (response.Contains("```"))
            {
                // Extract JSON content from markdown code blocks
                var startIndex = response.IndexOf("```json");
                if (startIndex >= 0)
                {
                    startIndex += 7; // Move past ```json
                    var endIndex = response.IndexOf("```", startIndex);
                    if (endIndex > startIndex)
                    {
                        response = response.Substring(startIndex, endIndex - startIndex).Trim();
                    }
                }
                else
                {
                    // Try without json marker
                    startIndex = response.IndexOf("```");
                    if (startIndex >= 0)
                    {
                        startIndex += 3; // Move past ```
                        var endIndex = response.IndexOf("```", startIndex);
                        if (endIndex > startIndex)
                        {
                            response = response.Substring(startIndex, endIndex - startIndex).Trim();
                        }
                    }
                }
            }
            
            // Ensure it's valid JSON
            response = response.Trim();
            if (!response.StartsWith("{") && !response.StartsWith("["))
            {
                // If it's not JSON, wrap it or return empty object
                _logger.LogWarning($"Response doesn't look like JSON: {response.Substring(0, Math.Min(100, response.Length))}");
                return "{}";
            }
            
            return response;
        }

        private string GenerateImageAnalysisPrompt(string extractedText = "")
        {
            return $@"
Analyze this food product image with EXTREME precision and extract every detail visible on the packaging.
{(string.IsNullOrEmpty(extractedText) ? "" : $"Additional context: {extractedText}")}

You must return a valid JSON response matching the ProductAnalysis schema with these exact fields:

{{
  ""productIdentification"": {{
    ""detectedProduct"": ""exact product name visible on package (e.g., 'Oreo Original')"",
    ""confidence"": 0.95,
    ""brandReference"": ""brand name (e.g., 'Oreo', 'Nabisco')"",
    ""genericName"": ""generic product type (e.g., 'sandwich cookies', 'cream-filled cookies')"",
    ""productVariant"": ""specific variant (e.g., 'Original', 'Double Stuf', 'Golden')""
  }},
  ""categoryClassification"": {{
    ""primaryCategory"": ""main category (e.g., 'Cookies', 'Snacks', 'Biscuits')"",
    ""secondaryCategory"": ""sub-category (e.g., 'Sandwich Cookies', 'Cream Cookies')"",
    ""marketSegment"": ""target market (e.g., 'Consumer', 'Retail', 'Food Service')""
  }},
  ""packagingDetails"": {{
    ""packageType"": ""type of packaging (e.g., 'Box', 'Pack', 'Wrapper')"",
    ""material"": ""packaging material (e.g., 'Cardboard', 'Plastic', 'Paper')"",
    ""netWeight"": ""weight shown (e.g., '176g', '6.2oz')"",
    ""unitsPerPackage"": ""number of units (e.g., '4 packs', '16 cookies')"",
    ""servingSize"": ""serving size from nutrition label"",
    ""servingsPerContainer"": ""number of servings""
  }},
  ""labelingInformation"": {{
    ""ingredients"": [""list"", ""all"", ""ingredients"", ""visible""],
    ""allergens"": [""wheat"", ""soy"", ""milk"", ""eggs"", ""nuts""],
    ""nutritionalInfo"": {{
      ""calories"": ""per serving"",
      ""fat"": ""grams"",
      ""sugar"": ""grams"",
      ""protein"": ""grams""
    }},
    ""manufacturerInfo"": ""company name and location"",
    ""countryOfOrigin"": ""manufacturing country if visible"",
    ""bestBeforeDate"": ""expiry or best before date format"",
    ""barcodes"": [""barcode numbers if visible""]
  }},
  ""productAttributes"": {{
    ""isKosher"": true/false,
    ""kosherCertification"": ""certification symbol (OU, OK, Star-K, etc.)"",
    ""isHalal"": true/false,
    ""halalCertification"": ""certification body if visible"",
    ""isOrganic"": true/false,
    ""organicCertification"": ""USDA Organic, EU Organic, etc."",
    ""isGlutenFree"": true/false,
    ""isVegan"": true/false,
    ""isVegetarian"": true/false,
    ""containsNuts"": true/false,
    ""containsDairy"": true/false,
    ""isSugarFree"": false,
    ""isLactoseFree"": false,
    ""otherCertifications"": [""list any other certifications""]
  }},
  ""visualElements"": {{
    ""dominantColors"": [""blue"", ""white"", ""brown""],
    ""logoPresent"": true/false,
    ""productImageShown"": true/false,
    ""designStyle"": ""modern, traditional, minimalist, etc.""
  }},
  ""detailedDescription"": {{
    ""summary"": ""Brief description of what you see (e.g., 'Oreo Original cookies in a blue package showing 4 individual packs')"",
    ""keyCharacteristics"": [
      ""Chocolate sandwich cookies"",
      ""Cream filling"",
      ""Multi-pack format"",
      ""Hebrew text indicates Israeli market""
    ],
    ""targetMarket"": ""intended market based on language and labeling"",
    ""qualityIndicators"": [""premium"", ""standard"", ""economy""],
    ""shelfLife"": ""estimated shelf life based on product type""
  }},
  ""marketingClaims"": {{
    ""claims"": [""Original"", ""4x packs"", ""any other marketing text""],
    ""promotions"": ""any promotional text or offers"",
    ""brandSlogans"": ""any slogans or taglines visible""
  }},
  ""supplierSearchTerms"": {{
    ""recommendedSearchTerms"": [
      ""cookies"",
      ""sandwich cookies"",
      ""chocolate cookies"",
      ""cream-filled cookies"",
      ""Oreo"",
      ""biscuits""
    ],
    ""supplierCategories"": [
      ""Cookies & Biscuits"",
      ""Snack Foods"",
      ""Confectionery"",
      ""Bakery Products""
    ],
    ""potentialSuppliers"": [
      ""Cookie manufacturers"",
      ""Snack distributors"",
      ""Confectionery suppliers""
    ]
  }}
}}

CRITICAL: Focus on extracting actual visible text and details from the package. For Hebrew/Arabic text, note the presence but focus on English text and universal symbols. Be extremely accurate with product names, weights, and certifications visible on the package.";
        }

        public async Task<ProductAnalysis> AnalyzeUrlRequest(string url)
        {
            try
            {
                _logger.LogInformation("Analyzing URL request: {Url}", url);

                // Fetch content from URL
                var content = await FetchUrlContent(url);

                // Analyze the content
                var prompt = GenerateUrlAnalysisPrompt(url, content);
                var analysisJson = await CallOpenAIApi(prompt);

                var analysis = JsonSerializer.Deserialize<ProductAnalysis>(analysisJson) ?? new ProductAnalysis();

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing URL request");
                throw;
            }
        }

        private async Task<string> CallOpenAIApi(string prompt)
        {
            // Check if we have Azure OpenAI or OpenAI configured
            if (_useAzureOpenAI)
            {
                return await CallAzureOpenAI(prompt);
            }
            else if (!string.IsNullOrEmpty(_openAiApiKey))
            {
                return await CallStandardOpenAI(prompt);
            }
            else
            {
                _logger.LogError("No AI service configured. Please configure Azure OpenAI or OpenAI API keys.");
                throw new InvalidOperationException("AI service not configured. Please add Azure OpenAI or OpenAI API keys to configuration.");
            }
        }

        private async Task<string> CallVisionAPI(string imageDataUrl)
        {
            try
            {
                if (_useAzureOpenAI)
                {
                    return await CallAzureVision(imageDataUrl);
                }
                else if (!string.IsNullOrEmpty(_openAiApiKey))
                {
                    return await CallOpenAIVision(imageDataUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Vision API");
            }

            // Return empty string instead of throwing to allow graceful fallback
            return string.Empty;
        }

        private async Task<string> CallOpenAIVision(string imageDataUrl)
        {
            try
            {
                var request = new
                {
                    model = "gpt-4-vision-preview", // GPT-4 Vision model
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = new object[]
                            {
                                new { type = "text", text = GenerateImageAnalysisPrompt() },
                                new { type = "image_url", image_url = new { url = imageDataUrl } }
                            }
                        }
                    },
                    max_tokens = 4096
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseData = JsonDocument.Parse(responseContent);
                    return responseData.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString() ?? "{}";
                }
                else
                {
                    _logger.LogError($"OpenAI Vision API error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI Vision API");
            }

            return "{}";
        }

        private async Task<string> CallAzureVision(string imageDataUrl)
        {
            try
            {
                _logger.LogInformation($"Calling Azure Vision with endpoint: {_azureOpenAiEndpoint}");
                
                var client = new Azure.AI.OpenAI.AzureOpenAIClient(
                    new Uri(_azureOpenAiEndpoint),
                    new AzureKeyCredential(_azureOpenAiKey));

                // Use gpt-4o model which has vision capabilities
                var deploymentName = _configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4o";
                _logger.LogInformation($"Using deployment: {deploymentName}");
                
                var chatClient = client.GetChatClient(deploymentName);

                // Convert base64 data URL to binary data
                BinaryData imageData;
                if (imageDataUrl.StartsWith("data:"))
                {
                    // Extract base64 data from data URL
                    var base64Data = imageDataUrl.Substring(imageDataUrl.IndexOf(",") + 1);
                    var imageBytes = Convert.FromBase64String(base64Data);
                    imageData = BinaryData.FromBytes(imageBytes);
                    
                    // Determine MIME type from data URL
                    var mimeType = "image/jpeg";
                    if (imageDataUrl.Contains("image/png"))
                        mimeType = "image/png";
                    else if (imageDataUrl.Contains("image/gif"))
                        mimeType = "image/gif";
                    else if (imageDataUrl.Contains("image/webp"))
                        mimeType = "image/webp";
                    
                    _logger.LogInformation($"Processing image data: {imageBytes.Length} bytes, type: {mimeType}");
                }
                else
                {
                    // If it's a regular URL, use it directly
                    imageData = BinaryData.FromString(imageDataUrl);
                }

                var messages = new List<OpenAI.Chat.ChatMessage>
                {
                    OpenAI.Chat.ChatMessage.CreateUserMessage(
                        OpenAI.Chat.ChatMessageContentPart.CreateTextPart(GenerateImageAnalysisPrompt()),
                        OpenAI.Chat.ChatMessageContentPart.CreateImagePart(imageData, "image/jpeg")
                    )
                };

                _logger.LogInformation("Sending image to Azure OpenAI for analysis...");
                var response = await chatClient.CompleteChatAsync(messages);

                if (response.Value != null && response.Value.Content.Count > 0)
                {
                    _logger.LogInformation("Successfully received response from Azure OpenAI Vision");
                    var responseText = response.Value.Content[0].Text ?? "{}";
                    
                    // Log the raw response for debugging
                    _logger.LogInformation($"Raw response from Azure OpenAI (first 500 chars): {responseText.Substring(0, Math.Min(500, responseText.Length))}");
                    
                    // Clean the response - remove markdown code blocks if present
                    responseText = CleanJsonResponse(responseText);
                    
                    // Log the cleaned response
                    _logger.LogInformation($"Cleaned JSON response (first 500 chars): {responseText.Substring(0, Math.Min(500, responseText.Length))}");
                    
                    return responseText;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling Azure Vision API: {ex.Message}");
                _logger.LogError($"Endpoint: {_azureOpenAiEndpoint}, Has Key: {!string.IsNullOrEmpty(_azureOpenAiKey)}");
            }

            return "{}";
        }

        private async Task<string> CallAzureOpenAI(string prompt)
        {
            try
            {
                var client = new Azure.AI.OpenAI.AzureOpenAIClient(
                    new Uri(_azureOpenAiEndpoint),
                    new AzureKeyCredential(_azureOpenAiKey));

                var chatClient = client.GetChatClient("gpt-35-turbo"); // Or gpt-4 if deployed

                var messages = new List<OpenAI.Chat.ChatMessage>
                {
                    new OpenAI.Chat.SystemChatMessage("You are a food product analyst. Analyze the product and return a JSON response."),
                    new OpenAI.Chat.UserChatMessage(prompt)
                };

                var response = await chatClient.CompleteChatAsync(messages);

                if (response.Value != null && response.Value.Content.Count > 0)
                {
                    var text = response.Value.Content[0].Text;
                    if (string.IsNullOrEmpty(text))
                    {
                        throw new InvalidOperationException("AI service returned empty response");
                    }
                    return text;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Azure OpenAI API");
            }

            throw new InvalidOperationException("Failed to call AI service. Please check your API configuration.");
        }

        private async Task<string> CallStandardOpenAI(string prompt)
        {
            try
            {
                var request = new
                {
                    model = "gpt-3.5-turbo", // Using GPT-3.5 for cost efficiency
                    messages = new[]
                    {
                        new { role = "system", content = "You are a food product analyst. Analyze the product and return a JSON response." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7,
                    max_tokens = 1500
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseData = JsonDocument.Parse(responseContent);
                    return responseData.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString() ?? "{}";
                }
                else
                {
                    _logger.LogError($"OpenAI API error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI API");
            }

            throw new InvalidOperationException("Failed to call AI service. Please check your API configuration.");
        }

        private async Task<string> FetchUrlContent(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching URL content");
            }

            return string.Empty;
        }

        private string GenerateTextAnalysisPrompt(string text)
        {
            return $@"
Analyze this product request and provide an EXTREMELY DETAILED product specification in JSON format:
'{text}'

Return a JSON object with ALL these sections (use null for unknown fields):

1. productIdentification:
   - detectedProduct: exact product name
   - confidence: 0-1 score
   - brandReference: specific brand if mentioned
   - genericName: generic product category

2. detailedDescription:
   - summary: comprehensive description
   - keyCharacteristics: array of specific features

3. technicalSpecifications:
   - productDimensions: exact measurements if known
   - composition: material composition
   - colorProfile: colors used
   - textureProfile: texture description

4. categoryClassification:
   - primaryCategory, secondaryCategory, specificType
   - alternativeNames: array of other names

5. commonAttributes:
   - typicalIngredients: full ingredient list
   - flavorNotes: taste profile
   - usageOccasions: when/how used
   - shelfLife: storage duration
   - certifications: Kosher, Halal, Organic, etc.

6. marketContext:
   - commonBrands: competitor brands
   - typicalPackaging: packaging description
   - marketPositioning: premium/budget/mainstream
   - priceSegment: price range

7. packagingDetails:
   - packageType: box/wrapper/bottle/can/pouch
   - material: plastic/cardboard/metal/glass
   - dimensions: LxWxH if known
   - netWeight: weight in grams
   - netWeightOz: weight in ounces
   - netVolume: volume in ml
   - netVolumeFlOz: volume in fl oz
   - unitsPerPackage: number of items
   - servingSize: per serving amount
   - servingsPerContainer: total servings
   - packagingComponents: array of package parts
   - isResealable: true/false
   - isRecyclable: true/false
   - recyclingCode: recycling number

8. labelingInformation:
   - productNameOnLabel: exact name on package
   - brandName: brand as shown
   - manufacturer: company name
   - manufacturerCompany: parent company
   - manufacturerWebsite: company URL
   - brandWebsite: brand URL
   - countryOfOrigin: where made
   - languages: array of languages on package
   - barcode: barcode number if known
   - sku: product SKU
   - ingredientsText: full ingredients as array
   - nutritionalInfo: nutrition facts as key-value pairs
   - allergens: allergen warnings array
   - certificationMarks: certification symbols array
   - bestBeforeDate: expiry format
   - storageInstructions: how to store
   - preparationInstructions: how to prepare
   - warnings: safety warnings array
   - marketingClaims: marketing text array
   - contactInformation: contact details
   - regulatoryText: legal text array

9. visualElements:
   - primaryColors: main colors array
   - logoDescription: logo details
   - imageDescriptions: package images array
   - designStyle: visual style
   - hasWindowDisplay: true/false
   - specialEffects: finish effects array
   - packageShape: shape description

10. productAttributes (EXTREMELY IMPORTANT):
   Dietary Certifications:
   - isKosher: true/false/null
   - kosherCertification: certification body name (OU, OK, Star-K, etc.)
   - isHalal: true/false/null
   - halalCertification: certification body name
   
   Allergen Information:
   - isGlutenFree, isNutFree, isDairyFree, isEggFree, isSoyFree, isShellFishFree: true/false/null
   - containsAllergens: array of allergens present
   - mayContainAllergens: array of cross-contamination warnings
   
   Sugar & Sweeteners:
   - isSugarFree, isNoSugarAdded, isDiabeticFriendly: true/false/null
   - sugarSubstitutes: array (stevia, aspartame, sucralose, etc.)
   - totalSugarContent: per serving amount
   
   Nutritional Enhancements:
   - isVitaminEnriched, isProteinEnriched, isFiberEnriched: true/false/null
   - addedVitamins: array of vitamins (A, B12, D, etc.)
   - proteinContent, fiberContent: per serving amounts
   - isCalciumFortified, isIronFortified: true/false/null
   - containsProbiotics, containsOmega3: true/false/null
   
   Dietary Preferences:
   - isVegan, isVegetarian, isPlantBased: true/false/null
   - isOrganic: true/false/null
   - organicCertification: USDA Organic, EU Organic, etc.
   - isNonGMO: true/false/null
   - isFairTrade, isLocallySourced: true/false/null
   
   Additional Attributes:
   - isNoPreservatives, isNoArtificialColors, isNoArtificialFlavors: true/false/null
   - isAllNatural, isMinimallyProcessed: true/false/null
   - packageTextClaims: array of ALL claims found on package
   - nutritionalClaims: array (""High in..."", ""Source of..."", etc.)
   - healthClaims: array (""Heart healthy"", ""Immune support"", etc.)
   - caloriesPerServing: exact calorie count

Be as specific and detailed as possible, especially for certifications, allergens, and nutritional attributes."";";
        }

        private string GenerateUrlAnalysisPrompt(string url, string content)
        {
            return $@"
Analyze this product from URL {url} and extract ALL available product details.
Page content: {content.Substring(0, Math.Min(2000, content.Length))}

Provide COMPREHENSIVE product analysis in JSON with ALL sections from GenerateTextAnalysisPrompt including:
- productIdentification
- detailedDescription  
- technicalSpecifications
- categoryClassification
- commonAttributes
- marketContext
- packagingDetails (extract ALL weight/volume/dimension information)
- labelingInformation (extract manufacturer details, website, all text)
- visualElements

Focus on extracting:
- Exact net weight/volume in multiple units (g, mg, ml, oz, fl oz)
- Package dimensions and material
- Manufacturer company name and website
- All nutritional information
- Complete ingredients list
- All certifications and regulatory marks

Be extremely detailed and specific.";
        }

    }
}
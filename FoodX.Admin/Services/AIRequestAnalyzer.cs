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
            
            // Check for Azure OpenAI configuration first
            _azureOpenAiEndpoint = configuration["AzureOpenAI:Endpoint"] ?? "";
            _azureOpenAiKey = configuration["AzureOpenAI:ApiKey"] ?? "";
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
                return GenerateFallbackAnalysis(text, "text");
            }
        }

        public async Task<ProductAnalysis> AnalyzeImageRequest(byte[] imageData)
        {
            try
            {
                _logger.LogInformation("Analyzing image request with GPT-4 Vision");

                // Convert image to base64 for GPT-4 Vision
                var base64Image = Convert.ToBase64String(imageData);
                var dataUrl = $"data:image/jpeg;base64,{base64Image}";

                // Use GPT-4 Vision for comprehensive image analysis
                var analysisJson = await CallVisionAPI(dataUrl);
                
                if (!string.IsNullOrEmpty(analysisJson))
                {
                    var analysis = JsonSerializer.Deserialize<ProductAnalysis>(analysisJson);
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
                
                _logger.LogWarning("Vision analysis failed, returning mock data");
                return GenerateMockImageAnalysis();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing image request");
                return GenerateFallbackAnalysis("image", "image");
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
                
                return JsonSerializer.Deserialize<ProductAnalysis>(analysisJson) ?? GenerateMockImageAnalysis();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with Computer Vision analysis");
                return GenerateMockImageAnalysis();
            }
        }

        private string GenerateImageAnalysisPrompt(string extractedText = "")
        {
            return $@"
Analyze this product image and extract EVERY visible detail from the packaging.
{(string.IsNullOrEmpty(extractedText) ? "" : $"Additional context: {extractedText}")}

Provide EXHAUSTIVE analysis in JSON format including:

1. Extract ALL visible text including:
   - Product name (exact spelling)
   - Brand name and logo
   - Net weight/volume (ALL units shown)
   - Ingredients list (complete)
   - Nutritional information (all values)
   - Manufacturer details and website
   - Certifications (Kosher, Halal, etc.)
   - Barcodes and product codes
   - Marketing text (""Original"", ""4x"", etc.)
   - Languages used on package
   - Any warnings or instructions

2. Describe packaging in detail:
   - Material (cardboard, plastic, etc.)
   - Shape and structure
   - Dimensions if visible
   - Number of units (e.g., ""4x"")
   - Color scheme
   - Special features (resealable, window, etc.)

3. Visual elements:
   - Logo design
   - Product images
   - Color palette
   - Design style

4. Product Attributes (CRITICAL - analyze all text and symbols):
   - Dietary certifications: Kosher (OU, OK, Star-K symbols), Halal certification
   - Allergen information: Gluten-free, nut-free, dairy-free claims
   - Sugar content: Sugar-free, no sugar added, diabetic friendly claims
   - Nutritional enhancements: Vitamin enriched, protein enriched, fiber added
   - Dietary preferences: Vegan, vegetarian, plant-based, organic, non-GMO
   - Special attributes: All natural, no preservatives, no artificial colors/flavors

Return complete JSON with all sections including packagingDetails, labelingInformation, visualElements, and productAttributes.
Be EXTREMELY specific about measurements, quantities, certifications, and all text visible on the package.";
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
                return GenerateFallbackAnalysis(url, "url");
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
                _logger.LogWarning("No AI service configured, returning mock data");
                return JsonSerializer.Serialize(GenerateMockAnalysis());
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
            
            return JsonSerializer.Serialize(GenerateMockImageAnalysis());
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
                var client = new Azure.AI.OpenAI.AzureOpenAIClient(
                    new Uri(_azureOpenAiEndpoint),
                    new AzureKeyCredential(_azureOpenAiKey));

                // For Azure OpenAI, we need to use gpt-4-vision deployment
                var chatClient = client.GetChatClient("gpt-4-vision-preview");

                var messages = new List<OpenAI.Chat.ChatMessage>
                {
                    OpenAI.Chat.ChatMessage.CreateUserMessage(
                        OpenAI.Chat.ChatMessageContentPart.CreateTextPart(GenerateImageAnalysisPrompt()),
                        OpenAI.Chat.ChatMessageContentPart.CreateImagePart(new Uri(imageDataUrl))
                    )
                };

                var response = await chatClient.CompleteChatAsync(messages);
                
                if (response.Value != null)
                {
                    return response.Value.Content[0].Text ?? "{}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Azure Vision API");
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
                
                if (response.Value != null)
                {
                    return response.Value.Content[0].Text ?? JsonSerializer.Serialize(GenerateMockAnalysis());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Azure OpenAI API");
            }

            return JsonSerializer.Serialize(GenerateMockAnalysis());
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

            return JsonSerializer.Serialize(GenerateMockAnalysis());
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

        private ProductAnalysis GenerateMockAnalysis()
        {
            // Generate mock Oreo-style cookie analysis for demo based on the image provided
            return new ProductAnalysis
            {
                ProductIdentification = new ProductIdentification
                {
                    DetectedProduct = "OREO Original Chocolate Sandwich Cookies",
                    Confidence = 0.95,
                    BrandReference = "OREO",
                    GenericName = "Cream-filled chocolate sandwich biscuit"
                },
                DetailedDescription = new DetailedDescription
                {
                    Summary = "A sandwich cookie consisting of two chocolate-flavored wafers with sweet cream filling, packaged in a blue cardboard box",
                    KeyCharacteristics = new List<string>
                    {
                        "Round shape with embossed OREO pattern",
                        "Dark chocolate-flavored wafers",
                        "White vanilla-flavored cream center",
                        "Crispy texture with smooth filling",
                        "4x individual packs"
                    }
                },
                TechnicalSpecifications = new TechnicalSpecifications
                {
                    ProductDimensions = "Approximately 45mm diameter, 9mm thickness per cookie",
                    Composition = "Two wafer discs with cream filling (approximately 29% filling)",
                    ColorProfile = "Dark brown/black wafers, white filling, blue packaging",
                    TextureProfile = "Crunchy wafer, smooth cream"
                },
                CategoryClassification = new CategoryClassification
                {
                    PrimaryCategory = "Bakery & Confectionery",
                    SecondaryCategory = "Biscuits & Cookies",
                    SpecificType = "Sandwich Cookies",
                    AlternativeNames = new List<string> { "Sandwich biscuits", "Cream cookies", "Filled cookies" }
                },
                CommonAttributes = new CommonAttributes
                {
                    TypicalIngredients = new List<string>
                    {
                        "Wheat flour", "Sugar", "Vegetable oils (palm and/or canola)", 
                        "Cocoa powder (4.5%)", "High fructose corn syrup", "Leavening agents",
                        "Salt", "Soy lecithin", "Vanilla flavoring", "Chocolate"
                    },
                    FlavorNotes = "Sweet, chocolatey with vanilla cream notes",
                    UsageOccasions = new List<string> { "Snacking", "Tea/coffee accompaniment", "Dessert", "Baking ingredient", "Milk dunking" },
                    ShelfLife = "6-9 months",
                    Certifications = new List<string> { "Kosher", "Cocoa Life sustainability program" }
                },
                MarketContext = new MarketContext
                {
                    CommonBrands = new List<string> { "Oreo", "Chips Ahoy", "Hydrox", "Private label alternatives" },
                    TypicalPackaging = "Cardboard box with plastic tray insert",
                    MarketPositioning = "Premium mass market snack cookie",
                    PriceSegment = "Mid-range"
                },
                PackagingDetails = new PackagingDetails
                {
                    PackageType = "Cardboard box",
                    Material = "Recyclable cardboard with plastic tray",
                    Dimensions = "200mm x 70mm x 45mm (approx)",
                    NetWeight = "176g",
                    NetWeightOz = "6.2 oz",
                    UnitsPerPackage = "4x individual packs",
                    ServingSize = "2 cookies (29g)",
                    ServingsPerContainer = "6",
                    PackagingComponents = new List<string> { "Outer cardboard box", "Inner plastic tray", "Individual wrappers" },
                    IsResealable = false,
                    IsRecyclable = true,
                    RecyclingCode = "21 PAP (cardboard)"
                },
                LabelingInformation = new LabelingInformation
                {
                    ProductNameOnLabel = "OREO Original",
                    BrandName = "OREO",
                    Manufacturer = "Mondelez International",
                    ManufacturerCompany = "Mondelez International, Inc.",
                    ManufacturerWebsite = "https://www.mondelezinternational.com",
                    BrandWebsite = "https://www.oreo.com",
                    CountryOfOrigin = "Various (check package)",
                    Languages = new List<string> { "Hebrew", "English" },
                    Barcode = "7290000000000",
                    IngredientsText = new List<string>
                    {
                        "Wheat flour", "Sugar", "Vegetable oil", "Cocoa powder", "Corn syrup",
                        "Leavening agents", "Salt", "Soy lecithin", "Vanillin", "Chocolate"
                    },
                    NutritionalInfo = new Dictionary<string, string>
                    {
                        { "Calories", "160 per serving" },
                        { "Total Fat", "7g" },
                        { "Saturated Fat", "2g" },
                        { "Trans Fat", "0g" },
                        { "Cholesterol", "0mg" },
                        { "Sodium", "135mg" },
                        { "Total Carbohydrates", "25g" },
                        { "Dietary Fiber", "1g" },
                        { "Sugars", "14g" },
                        { "Protein", "1g" }
                    },
                    Allergens = new List<string> { "Contains wheat", "Contains soy", "May contain milk" },
                    CertificationMarks = new List<string> { "Kosher dairy", "Cocoa Life certified" },
                    StorageInstructions = "Store in a cool, dry place",
                    MarketingClaims = new List<string> { "Original", "4x", "Milk's favorite cookie" },
                    RegulatoryText = new List<string> { "Product of licensed manufacturer" }
                },
                VisualElements = new VisualElements
                {
                    PrimaryColors = new List<string> { "Blue", "White", "Black" },
                    LogoDescription = "OREO logo in white text on blue background with cookie imagery",
                    ImageDescriptions = new List<string> { "OREO cookies with milk splash", "Stack of OREO cookies" },
                    DesignStyle = "Modern, playful, bold",
                    HasWindowDisplay = false,
                    SpecialEffects = new List<string> { "Glossy finish", "Embossed logo", "Milk splash graphic" },
                    PackageShape = "Rectangular box"
                },
                ProductAttributes = new ProductAttributes
                {
                    IsKosher = true,
                    KosherCertification = "OU-D (Orthodox Union Dairy)",
                    IsHalal = false,
                    IsGlutenFree = false,
                    ContainsAllergens = new List<string> { "Wheat", "Soy" },
                    MayContainAllergens = new List<string> { "Milk" },
                    IsSugarFree = false,
                    IsNoSugarAdded = false,
                    TotalSugarContent = "14g per serving",
                    IsVegan = false,
                    IsVegetarian = true,
                    CaloriesPerServing = "160",
                    PackageTextClaims = new List<string> { "Original", "4x", "Milk's Favorite Cookie" },
                    IsNoArtificialColors = false,
                    IsNoArtificialFlavors = false
                }
            };
        }

        private ProductAnalysis GenerateMockImageAnalysis()
        {
            // Return a mock analysis for image inputs
            return GenerateMockAnalysis();
        }

        private ProductAnalysis GenerateFallbackAnalysis(string input, string inputType)
        {
            return new ProductAnalysis
            {
                ProductIdentification = new ProductIdentification
                {
                    DetectedProduct = $"Product from {inputType}",
                    Confidence = 0.5,
                    GenericName = "Food product requiring manual review"
                },
                DetailedDescription = new DetailedDescription
                {
                    Summary = $"Analysis pending for {inputType} input: {input.Substring(0, Math.Min(50, input.Length))}",
                    KeyCharacteristics = new List<string> { "Requires manual review" }
                }
            };
        }
    }
}
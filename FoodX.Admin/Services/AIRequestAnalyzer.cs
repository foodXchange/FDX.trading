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
                _logger.LogInformation("Analyzing image request");

                // Check if Azure Computer Vision is configured
                var visionEndpoint = _configuration["AzureComputerVision:Endpoint"];
                var visionKey = _configuration["AzureComputerVision:ApiKey"];

                if (!string.IsNullOrEmpty(visionEndpoint) && !string.IsNullOrEmpty(visionKey))
                {
                    return await AnalyzeWithComputerVision(imageData, visionEndpoint, visionKey);
                }
                
                // For now, return a mock analysis if Computer Vision not configured
                _logger.LogWarning("Azure Computer Vision not configured, returning mock data");
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

        private string GenerateImageAnalysisPrompt(string extractedText)
        {
            return $@"
Analyze this product based on image analysis results:
Extracted text and features: {extractedText}

Return a JSON object with these sections:
- productIdentification (detectedProduct, confidence, brandReference, genericName)
- detailedDescription (summary, keyCharacteristics array)
- technicalSpecifications (productDimensions, composition, colorProfile, textureProfile)
- categoryClassification (primaryCategory, secondaryCategory, specificType, alternativeNames array)
- commonAttributes (typicalIngredients array, flavorNotes, usageOccasions array, shelfLife, certifications array)
- marketContext (commonBrands array, typicalPackaging, marketPositioning, priceSegment)";
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
Analyze this product request and provide a detailed product specification in JSON format:
'{text}'

Return a JSON object with these sections:
- productIdentification (detectedProduct, confidence, brandReference, genericName)
- detailedDescription (summary, keyCharacteristics array)
- technicalSpecifications (productDimensions, composition, colorProfile, textureProfile)
- categoryClassification (primaryCategory, secondaryCategory, specificType, alternativeNames array)
- commonAttributes (typicalIngredients array, flavorNotes, usageOccasions array, shelfLife, certifications array)
- marketContext (commonBrands array, typicalPackaging, marketPositioning, priceSegment)

Be specific and detailed in your analysis.";
        }

        private string GenerateUrlAnalysisPrompt(string url, string content)
        {
            return $@"
Analyze this product from URL {url} and provide a detailed product specification in JSON format.
Page content summary: {content.Substring(0, Math.Min(1000, content.Length))}

Return a JSON object with these sections:
- productIdentification (detectedProduct, confidence, brandReference, genericName)
- detailedDescription (summary, keyCharacteristics array)
- technicalSpecifications (productDimensions, composition, colorProfile, textureProfile)
- categoryClassification (primaryCategory, secondaryCategory, specificType, alternativeNames array)
- commonAttributes (typicalIngredients array, flavorNotes, usageOccasions array, shelfLife, certifications array)
- marketContext (commonBrands array, typicalPackaging, marketPositioning, priceSegment)";
        }

        private ProductAnalysis GenerateMockAnalysis()
        {
            // Generate mock Oreo-style cookie analysis for demo
            return new ProductAnalysis
            {
                ProductIdentification = new ProductIdentification
                {
                    DetectedProduct = "Chocolate Sandwich Cookie",
                    Confidence = 0.95,
                    BrandReference = "Oreo-style",
                    GenericName = "Cream-filled chocolate sandwich biscuit"
                },
                DetailedDescription = new DetailedDescription
                {
                    Summary = "A sandwich cookie consisting of two chocolate-flavored wafers with sweet cream filling",
                    KeyCharacteristics = new List<string>
                    {
                        "Round shape with embossed pattern",
                        "Dark chocolate-flavored wafers",
                        "White vanilla-flavored cream center",
                        "Crispy texture with smooth filling"
                    }
                },
                TechnicalSpecifications = new TechnicalSpecifications
                {
                    ProductDimensions = "Approximately 45mm diameter, 9mm thickness",
                    Composition = "Two wafer discs with cream filling (approximately 30% filling)",
                    ColorProfile = "Dark brown/black wafers, white filling",
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
                        "Wheat flour", "Sugar", "Vegetable oils", "Cocoa powder (4-7%)",
                        "Corn syrup", "Leavening agents", "Vanilla flavoring"
                    },
                    FlavorNotes = "Sweet, chocolatey with vanilla cream notes",
                    UsageOccasions = new List<string> { "Snacking", "Tea/coffee accompaniment", "Dessert", "Baking ingredient" },
                    ShelfLife = "6-9 months",
                    Certifications = new List<string> { "Kosher", "Halal options available" }
                },
                MarketContext = new MarketContext
                {
                    CommonBrands = new List<string> { "Oreo", "Private label alternatives", "Hydrox" },
                    TypicalPackaging = "Plastic trays, flow wrap, or rigid containers",
                    MarketPositioning = "Mass market snack cookie",
                    PriceSegment = "Mid-range"
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
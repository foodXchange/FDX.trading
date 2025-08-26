using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using OpenAI.Embeddings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FoodX.Core.Services
{
    public interface IEmbeddingService
    {
        Task<float[]?> GetEmbeddingAsync(string text);
        Task<List<float[]>> GetEmbeddingsAsync(List<string> texts);
        Task<bool> UpdateAllProductEmbeddingsAsync();
        Task<bool> UpdateAllCompanyEmbeddingsAsync();
    }

    public class AzureOpenAIEmbeddingService : IEmbeddingService
    {
        private readonly ILogger<AzureOpenAIEmbeddingService> _logger;
        private readonly IConfiguration _configuration;
        private readonly EmbeddingClient? _embeddingClient;
        private readonly string _deploymentName;
        private readonly int _dimensions;

        public AzureOpenAIEmbeddingService(
            IConfiguration configuration,
            ILogger<AzureOpenAIEmbeddingService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Get Azure OpenAI configuration
            var endpoint = configuration["AzureOpenAI:Endpoint"];
            var apiKey = configuration["AzureOpenAI:ApiKey"];
            _deploymentName = configuration["AzureOpenAI:EmbeddingDeployment"] ?? "text-embedding-ada-002";
            _dimensions = int.TryParse(configuration["AzureOpenAI:Dimensions"], out var dims) ? dims : 1536;

            if (string.IsNullOrEmpty(endpoint) || endpoint.Contains("your-openai-resource"))
            {
                _logger.LogWarning("AzureOpenAI:Endpoint is not configured. Embedding service will return null.");
                return; // Don't initialize the client if not configured
            }

            // Initialize Azure OpenAI client
            if (!string.IsNullOrEmpty(apiKey))
            {
                // Use API key authentication
                var client = new Azure.AI.OpenAI.AzureOpenAIClient(
                    new Uri(endpoint),
                    new AzureKeyCredential(apiKey));
                _embeddingClient = client.GetEmbeddingClient(_deploymentName);
            }
            else
            {
                // Use managed identity
                var client = new Azure.AI.OpenAI.AzureOpenAIClient(
                    new Uri(endpoint),
                    new DefaultAzureCredential());
                _embeddingClient = client.GetEmbeddingClient(_deploymentName);
            }

            _logger.LogInformation($"Initialized Azure OpenAI embedding service with deployment: {_deploymentName}");
        }

        public async Task<float[]?> GetEmbeddingAsync(string text)
        {
            try
            {
                if (_embeddingClient == null)
                {
                    _logger.LogWarning("Embedding client not initialized. Azure OpenAI not configured.");
                    return null;
                }

                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogWarning("Attempted to get embedding for empty text");
                    return null;
                }

                // Truncate text if it's too long (max ~8000 tokens for ada-002)
                if (text.Length > 30000)
                {
                    text = text.Substring(0, 30000);
                    _logger.LogWarning("Text truncated to 30000 characters for embedding");
                }

                var response = await _embeddingClient.GenerateEmbeddingAsync(text);

                if (response.Value != null)
                {
                    var embedding = response.Value.ToFloats().ToArray();
                    _logger.LogDebug($"Generated embedding with {embedding.Length} dimensions");
                    return embedding;
                }

                _logger.LogWarning("No embedding returned from Azure OpenAI");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating embedding");
                return null;
            }
        }

        public async Task<List<float[]>> GetEmbeddingsAsync(List<string> texts)
        {
            var embeddings = new List<float[]>();

            if (_embeddingClient == null)
            {
                _logger.LogWarning("Embedding client not initialized. Azure OpenAI not configured.");
                return embeddings;
            }

            try
            {
                // Process in batches (Azure OpenAI supports batch requests)
                const int batchSize = 16;

                for (int i = 0; i < texts.Count; i += batchSize)
                {
                    var batch = texts.Skip(i).Take(batchSize).ToList();

                    // Truncate texts if needed
                    var processedBatch = batch.Select(t =>
                        t?.Length > 30000 ? t.Substring(0, 30000) : t ?? string.Empty
                    ).ToList();

                    var response = await _embeddingClient.GenerateEmbeddingsAsync(processedBatch);

                    foreach (var item in response.Value)
                    {
                        embeddings.Add(item.ToFloats().ToArray());
                    }

                    _logger.LogInformation($"Processed batch {i / batchSize + 1}, generated {response.Value.Count} embeddings");

                    // Add delay to respect rate limits
                    if (i + batchSize < texts.Count)
                    {
                        await Task.Delay(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating batch embeddings");
            }

            return embeddings;
        }

        public async Task<bool> UpdateAllProductEmbeddingsAsync()
        {
            try
            {
                _logger.LogInformation("Starting batch update of product embeddings");

                // This would typically fetch products from database and update embeddings
                // For now, returning true as a placeholder
                // In production, this would:
                // 1. Fetch all active products
                // 2. Generate embeddings for names and descriptions
                // 3. Store in vector store

                await Task.CompletedTask;

                _logger.LogInformation("Completed batch update of product embeddings");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product embeddings");
                return false;
            }
        }

        public async Task<bool> UpdateAllCompanyEmbeddingsAsync()
        {
            try
            {
                _logger.LogInformation("Starting batch update of company embeddings");

                // This would typically fetch companies from database and update embeddings
                // For now, returning true as a placeholder

                await Task.CompletedTask;

                _logger.LogInformation("Completed batch update of company embeddings");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company embeddings");
                return false;
            }
        }
    }

    // Alternative implementation using direct HTTP calls (if Azure.AI.OpenAI package is not available)
    public class OpenAIEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenAIEmbeddingService> _logger;
        private readonly string _apiKey;
        private readonly string _model;

        public OpenAIEmbeddingService(
            IConfiguration configuration,
            ILogger<OpenAIEmbeddingService> logger,
            HttpClient httpClient)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentException("OpenAI:ApiKey not configured");
            _model = configuration["OpenAI:EmbeddingModel"] ?? "text-embedding-ada-002";

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<float[]?> GetEmbeddingAsync(string text)
        {
            try
            {
                var request = new
                {
                    model = _model,
                    input = text
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/embeddings", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<OpenAIEmbeddingResponse>(responseJson);

                    if (result?.data?.Count > 0)
                    {
                        return result.data[0].embedding;
                    }
                }
                else
                {
                    _logger.LogError($"OpenAI API error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI API");
            }

            return null;
        }

        public async Task<List<float[]>> GetEmbeddingsAsync(List<string> texts)
        {
            var embeddings = new List<float[]>();

            foreach (var text in texts)
            {
                var embedding = await GetEmbeddingAsync(text);
                if (embedding != null)
                {
                    embeddings.Add(embedding);
                }

                // Rate limiting
                await Task.Delay(100);
            }

            return embeddings;
        }

        public Task<bool> UpdateAllProductEmbeddingsAsync()
        {
            // Implement batch update logic
            return Task.FromResult(true);
        }

        public Task<bool> UpdateAllCompanyEmbeddingsAsync()
        {
            // Implement batch update logic
            return Task.FromResult(true);
        }

        private class OpenAIEmbeddingResponse
        {
            public List<EmbeddingData>? data { get; set; }
        }

        private class EmbeddingData
        {
            public float[]? embedding { get; set; }
        }
    }
}
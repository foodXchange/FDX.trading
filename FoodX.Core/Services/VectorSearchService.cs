using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FoodX.Core.Services
{
    public interface IVectorSearchService
    {
        Task<bool> StoreVectorAsync(string entityType, int entityId, string vectorType, float[] vector);
        Task<List<VectorSearchResult>> SearchAsync(string entityType, string vectorType, float[] searchVector, int topN = 10, float minSimilarity = 0.5f);
        Task<List<SimilarProductResult>> FindSimilarProductsAsync(int productId, int topN = 5);
        Task<bool> UpdateProductEmbeddingsAsync(int productId, float[] nameEmbedding, float[] descriptionEmbedding);
        Task<bool> UpdateCompanyEmbeddingsAsync(int companyId, float[] embedding);
    }

    public class VectorSearchService : IVectorSearchService
    {
        private readonly string _connectionString;
        private readonly ILogger<VectorSearchService> _logger;
        private readonly IEmbeddingService _embeddingService;

        public VectorSearchService(
            IConfiguration configuration,
            ILogger<VectorSearchService> logger,
            IEmbeddingService embeddingService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException("DefaultConnection string not found");
            _logger = logger;
            _embeddingService = embeddingService;
        }

        public async Task<bool> StoreVectorAsync(string entityType, int entityId, string vectorType, float[] vector)
        {
            try
            {
                var vectorJson = JsonSerializer.Serialize(vector);
                
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = new SqlCommand("sp_InsertVector", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                
                command.Parameters.AddWithValue("@EntityType", entityType);
                command.Parameters.AddWithValue("@EntityId", entityId);
                command.Parameters.AddWithValue("@VectorType", vectorType);
                command.Parameters.AddWithValue("@VectorData", vectorJson);
                command.Parameters.AddWithValue("@Dimensions", vector.Length);
                
                await command.ExecuteNonQueryAsync();
                
                _logger.LogInformation($"Stored vector for {entityType} {entityId} ({vectorType})");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error storing vector for {entityType} {entityId}");
                return false;
            }
        }

        public async Task<List<VectorSearchResult>> SearchAsync(
            string entityType, 
            string vectorType, 
            float[] searchVector, 
            int topN = 10, 
            float minSimilarity = 0.5f)
        {
            var results = new List<VectorSearchResult>();
            
            try
            {
                var vectorJson = JsonSerializer.Serialize(searchVector);
                
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = new SqlCommand("sp_VectorSearch", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                
                command.Parameters.AddWithValue("@SearchVector", vectorJson);
                command.Parameters.AddWithValue("@EntityType", entityType);
                command.Parameters.AddWithValue("@VectorType", vectorType);
                command.Parameters.AddWithValue("@TopN", topN);
                command.Parameters.AddWithValue("@MinSimilarity", minSimilarity);
                
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    results.Add(new VectorSearchResult
                    {
                        EntityId = reader.GetInt32(reader.GetOrdinal("EntityId")),
                        Similarity = Convert.ToSingle(reader.GetDouble(reader.GetOrdinal("Similarity"))),
                        EntityName = reader.IsDBNull(reader.GetOrdinal("EntityName")) 
                            ? null : reader.GetString(reader.GetOrdinal("EntityName")),
                        EntityDescription = reader.IsDBNull(reader.GetOrdinal("EntityDescription")) 
                            ? null : reader.GetString(reader.GetOrdinal("EntityDescription"))
                    });
                }
                
                _logger.LogInformation($"Vector search returned {results.Count} results for {entityType}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error performing vector search for {entityType}");
            }
            
            return results;
        }

        public async Task<List<SimilarProductResult>> FindSimilarProductsAsync(int productId, int topN = 5)
        {
            var results = new List<SimilarProductResult>();
            
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = new SqlCommand("sp_FindSimilarProducts", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                
                command.Parameters.AddWithValue("@ProductId", productId);
                command.Parameters.AddWithValue("@TopN", topN);
                
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    results.Add(new SimilarProductResult
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Category = reader.GetString(reader.GetOrdinal("Category")),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) 
                            ? null : reader.GetString(reader.GetOrdinal("Description")),
                        Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                        Similarity = Convert.ToSingle(reader.GetDouble(reader.GetOrdinal("Similarity")))
                    });
                }
                
                _logger.LogInformation($"Found {results.Count} similar products for product {productId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error finding similar products for {productId}");
            }
            
            return results;
        }

        public async Task<bool> UpdateProductEmbeddingsAsync(int productId, float[] nameEmbedding, float[] descriptionEmbedding)
        {
            try
            {
                // Store name embedding
                await StoreVectorAsync("Product", productId, "name", nameEmbedding);
                
                // Store description embedding
                await StoreVectorAsync("Product", productId, "description", descriptionEmbedding);
                
                // Create combined embedding (average of name and description)
                var combinedEmbedding = new float[nameEmbedding.Length];
                for (int i = 0; i < nameEmbedding.Length; i++)
                {
                    combinedEmbedding[i] = (nameEmbedding[i] + descriptionEmbedding[i]) / 2f;
                }
                
                await StoreVectorAsync("Product", productId, "combined", combinedEmbedding);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating embeddings for product {productId}");
                return false;
            }
        }

        public async Task<bool> UpdateCompanyEmbeddingsAsync(int companyId, float[] embedding)
        {
            try
            {
                await StoreVectorAsync("Company", companyId, "combined", embedding);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating embeddings for company {companyId}");
                return false;
            }
        }
    }

    public class VectorSearchResult
    {
        public int EntityId { get; set; }
        public float Similarity { get; set; }
        public string? EntityName { get; set; }
        public string? EntityDescription { get; set; }
    }

    public class SimilarProductResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public float Similarity { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodX.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodX.Admin.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VectorSearchController : ControllerBase
    {
        private readonly IVectorSearchService _vectorSearchService;
        private readonly IEmbeddingService _embeddingService;
        private readonly ILogger<VectorSearchController> _logger;

        public VectorSearchController(
            IVectorSearchService vectorSearchService,
            IEmbeddingService embeddingService,
            ILogger<VectorSearchController> logger)
        {
            _vectorSearchService = vectorSearchService;
            _embeddingService = embeddingService;
            _logger = logger;
        }

        [HttpPost("search/products")]
        public async Task<IActionResult> SearchProducts([FromBody] SearchRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Query))
                {
                    return BadRequest("Search query is required");
                }

                // Generate embedding for the search query
                var embedding = await _embeddingService.GetEmbeddingAsync(request.Query);

                if (embedding == null)
                {
                    _logger.LogWarning("Failed to generate embedding for search query");
                    return StatusCode(500, "Failed to process search query");
                }

                // Perform vector search
                var results = await _vectorSearchService.SearchAsync(
                    "Product",
                    "combined",
                    embedding,
                    request.TopN ?? 10,
                    request.MinSimilarity ?? 0.5f
                );

                return Ok(new
                {
                    query = request.Query,
                    resultCount = results.Count,
                    results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products");
                return StatusCode(500, "An error occurred while searching");
            }
        }

        [HttpPost("search/companies")]
        public async Task<IActionResult> SearchCompanies([FromBody] SearchRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Query))
                {
                    return BadRequest("Search query is required");
                }

                // Generate embedding for the search query
                var embedding = await _embeddingService.GetEmbeddingAsync(request.Query);

                if (embedding == null)
                {
                    _logger.LogWarning("Failed to generate embedding for search query");
                    return StatusCode(500, "Failed to process search query");
                }

                // Perform vector search
                var results = await _vectorSearchService.SearchAsync(
                    "Company",
                    "combined",
                    embedding,
                    request.TopN ?? 10,
                    request.MinSimilarity ?? 0.5f
                );

                return Ok(new
                {
                    query = request.Query,
                    resultCount = results.Count,
                    results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching companies");
                return StatusCode(500, "An error occurred while searching");
            }
        }

        [HttpGet("similar-products/{productId}")]
        public async Task<IActionResult> GetSimilarProducts(int productId, [FromQuery] int topN = 5)
        {
            try
            {
                var results = await _vectorSearchService.FindSimilarProductsAsync(productId, topN);

                return Ok(new
                {
                    productId = productId,
                    similarProducts = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error finding similar products for {productId}");
                return StatusCode(500, "An error occurred while finding similar products");
            }
        }

        [HttpPost("update-embeddings/product/{productId}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateProductEmbeddings(int productId)
        {
            try
            {
                // In a real implementation, fetch product details from database
                // For now, using placeholder text
                var productName = $"Product {productId}";
                var productDescription = $"Description for product {productId}";

                var nameEmbedding = await _embeddingService.GetEmbeddingAsync(productName);
                var descriptionEmbedding = await _embeddingService.GetEmbeddingAsync(productDescription);

                if (nameEmbedding == null || descriptionEmbedding == null)
                {
                    return StatusCode(500, "Failed to generate embeddings");
                }

                var success = await _vectorSearchService.UpdateProductEmbeddingsAsync(
                    productId,
                    nameEmbedding,
                    descriptionEmbedding
                );

                if (success)
                {
                    return Ok(new { message = "Product embeddings updated successfully" });
                }
                else
                {
                    return StatusCode(500, "Failed to store embeddings");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating embeddings for product {productId}");
                return StatusCode(500, "An error occurred while updating embeddings");
            }
        }

        [HttpPost("update-embeddings/all-products")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateAllProductEmbeddings()
        {
            try
            {
                _logger.LogInformation("Starting batch update of all product embeddings");

                var success = await _embeddingService.UpdateAllProductEmbeddingsAsync();

                if (success)
                {
                    return Ok(new { message = "All product embeddings updated successfully" });
                }
                else
                {
                    return StatusCode(500, "Failed to update all product embeddings");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating all product embeddings");
                return StatusCode(500, "An error occurred while updating embeddings");
            }
        }

        [HttpPost("update-embeddings/all-companies")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateAllCompanyEmbeddings()
        {
            try
            {
                _logger.LogInformation("Starting batch update of all company embeddings");

                var success = await _embeddingService.UpdateAllCompanyEmbeddingsAsync();

                if (success)
                {
                    return Ok(new { message = "All company embeddings updated successfully" });
                }
                else
                {
                    return StatusCode(500, "Failed to update all company embeddings");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating all company embeddings");
                return StatusCode(500, "An error occurred while updating embeddings");
            }
        }
    }

    public class SearchRequest
    {
        public string Query { get; set; } = string.Empty;
        public int? TopN { get; set; }
        public float? MinSimilarity { get; set; }
    }
}
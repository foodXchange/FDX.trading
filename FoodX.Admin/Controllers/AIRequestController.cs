using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using FoodX.Admin.Data;
using FoodX.Admin.Models;
using FoodX.Admin.Services;
using System.Text.Json;

namespace FoodX.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AIRequestController : ControllerBase
    {
        private readonly FoodXDbContext _context;
        private readonly IAIRequestAnalyzer _aiAnalyzer;
        private readonly ILogger<AIRequestController> _logger;

        public AIRequestController(
            FoodXDbContext context,
            IAIRequestAnalyzer aiAnalyzer,
            ILogger<AIRequestController> logger)
        {
            _context = context;
            _aiAnalyzer = aiAnalyzer;
            _logger = logger;
        }

        /// <summary>
        /// Get all buyer requests
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRequests([FromQuery] int? buyerId = null, [FromQuery] string? status = null)
        {
            var query = _context.BuyerRequests
                .Include(r => r.AnalysisResults)
                .AsQueryable();

            if (buyerId.HasValue)
            {
                query = query.Where(r => r.BuyerId == buyerId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }

            var requests = await query
                .OrderByDescending(r => r.CreatedAt)
                .Take(50)
                .ToListAsync();

            return Ok(requests);
        }

        /// <summary>
        /// Get a specific request by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRequest(int id)
        {
            var request = await _context.BuyerRequests
                .Include(r => r.AnalysisResults)
                .Include(r => r.Buyer)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            return Ok(request);
        }

        /// <summary>
        /// Create a new request with text input
        /// </summary>
        [HttpPost("analyze-text")]
        public async Task<IActionResult> AnalyzeText([FromBody] TextAnalysisRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Create request
                var request = new BuyerRequest
                {
                    BuyerId = model.BuyerId,
                    Title = model.Title,
                    InputType = InputTypes.Text,
                    InputContent = model.Text,
                    Status = RequestStatus.Processing,
                    Notes = model.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.BuyerRequests.Add(request);
                await _context.SaveChangesAsync();

                // Analyze with AI
                var analysis = await _aiAnalyzer.AnalyzeTextRequest(model.Text);

                // Save analysis result
                var analysisResult = new AIAnalysisResult
                {
                    RequestId = request.Id,
                    ConfidenceScore = analysis.ProductIdentification?.Confidence != null 
                        ? (decimal)(analysis.ProductIdentification.Confidence * 100) 
                        : 50,
                    AIProvider = "OpenAI",
                    ProcessedAt = DateTime.UtcNow
                };
                analysisResult.SetAnalysisData(analysis);

                _context.AIAnalysisResults.Add(analysisResult);
                
                // Update request status
                request.Status = RequestStatus.Analyzed;
                request.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    RequestId = request.Id,
                    Analysis = analysis,
                    Status = request.Status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing text request");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Create a new request with URL input
        /// </summary>
        [HttpPost("analyze-url")]
        public async Task<IActionResult> AnalyzeUrl([FromBody] UrlAnalysisRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!Uri.IsWellFormedUriString(model.Url, UriKind.Absolute))
            {
                return BadRequest("Invalid URL format");
            }

            try
            {
                // Create request
                var request = new BuyerRequest
                {
                    BuyerId = model.BuyerId,
                    Title = model.Title,
                    InputType = InputTypes.URL,
                    InputContent = model.Url,
                    Status = RequestStatus.Processing,
                    Notes = model.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.BuyerRequests.Add(request);
                await _context.SaveChangesAsync();

                // Analyze with AI
                var analysis = await _aiAnalyzer.AnalyzeUrlRequest(model.Url);

                // Save analysis result
                var analysisResult = new AIAnalysisResult
                {
                    RequestId = request.Id,
                    ConfidenceScore = analysis.ProductIdentification?.Confidence != null 
                        ? (decimal)(analysis.ProductIdentification.Confidence * 100) 
                        : 50,
                    AIProvider = "OpenAI",
                    ProcessedAt = DateTime.UtcNow
                };
                analysisResult.SetAnalysisData(analysis);

                _context.AIAnalysisResults.Add(analysisResult);
                
                // Update request status
                request.Status = RequestStatus.Analyzed;
                request.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    RequestId = request.Id,
                    Analysis = analysis,
                    Status = request.Status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing URL request");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Create a new request with image input
        /// </summary>
        [HttpPost("analyze-image")]
        public async Task<IActionResult> AnalyzeImage([FromForm] ImageAnalysisRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model.Image == null || model.Image.Length == 0)
            {
                return BadRequest("No image provided");
            }

            try
            {
                // Read image data
                using var memoryStream = new MemoryStream();
                await model.Image.CopyToAsync(memoryStream);
                var imageData = memoryStream.ToArray();

                // Create request
                var request = new BuyerRequest
                {
                    BuyerId = model.BuyerId,
                    Title = model.Title,
                    InputType = InputTypes.Image,
                    InputContent = $"Image: {model.Image.FileName}",
                    Status = RequestStatus.Processing,
                    Notes = model.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.BuyerRequests.Add(request);
                await _context.SaveChangesAsync();

                // Analyze with AI
                var analysis = await _aiAnalyzer.AnalyzeImageRequest(imageData);

                // Save analysis result
                var analysisResult = new AIAnalysisResult
                {
                    RequestId = request.Id,
                    ConfidenceScore = analysis.ProductIdentification?.Confidence != null 
                        ? (decimal)(analysis.ProductIdentification.Confidence * 100) 
                        : 50,
                    AIProvider = "Azure Computer Vision",
                    ProcessedAt = DateTime.UtcNow
                };
                analysisResult.SetAnalysisData(analysis);

                _context.AIAnalysisResults.Add(analysisResult);
                
                // Update request status
                request.Status = RequestStatus.Analyzed;
                request.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    RequestId = request.Id,
                    Analysis = analysis,
                    Status = request.Status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing image request");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Update request status
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest model)
        {
            var request = await _context.BuyerRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            request.Status = model.Status;
            request.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(request);
        }

        /// <summary>
        /// Delete a request
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var request = await _context.BuyerRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            _context.BuyerRequests.Remove(request);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // Request models
    public class TextAnalysisRequest
    {
        public int BuyerId { get; set; }
        public string Title { get; set; } = "";
        public string Text { get; set; } = "";
        public string? Notes { get; set; }
    }

    public class UrlAnalysisRequest
    {
        public int BuyerId { get; set; }
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public string? Notes { get; set; }
    }

    public class ImageAnalysisRequest
    {
        public int BuyerId { get; set; }
        public string Title { get; set; } = "";
        public IFormFile? Image { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = "";
    }
}
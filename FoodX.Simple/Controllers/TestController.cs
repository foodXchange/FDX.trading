using FoodX.Simple.Models;
using FoodX.Simple.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodX.Simple.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IProductBriefService _briefService;
        private readonly ILogger<TestController> _logger;

        public TestController(IProductBriefService briefService, ILogger<TestController> logger)
        {
            _briefService = briefService;
            _logger = logger;
        }

        [HttpPost("workflow")]
        public async Task<IActionResult> TestWorkflow()
        {
            try
            {
                _logger.LogInformation("Starting ProductBrief workflow test via API...");

                // Create test ProductBrief with exact data specified in test requirements
                var testBrief = new ProductBrief
                {
                    ProductName = "Organic Quinoa",
                    Category = "Grains",
                    PackageSize = "25kg bags",
                    CountryOfOrigin = "Peru",
                    IsKosherCertified = true,
                    KosherOrganization = "OU",
                    AdditionalNotes = "Premium quality organic quinoa for test workflow",
                    CreatedBy = "test-user",
                    CreatedDate = DateTime.UtcNow,
                    Status = "Active"
                };

                _logger.LogInformation("Creating ProductBrief: {ProductName}", testBrief.ProductName);

                // This should automatically create RFQ and Project via the workflow
                var result = await _briefService.CreateBriefAsync(testBrief);

                _logger.LogInformation("ProductBrief created with ID: {Id}", result.Id);

                return Ok(new
                {
                    success = true,
                    message = "ProductBrief created successfully! Automatic workflow should have generated RFQ and Project.",
                    productBriefId = result.Id,
                    productName = result.ProductName,
                    category = result.Category,
                    packageSize = result.PackageSize,
                    countryOfOrigin = result.CountryOfOrigin,
                    isKosherCertified = result.IsKosherCertified,
                    kosherOrganization = result.KosherOrganization,
                    status = result.Status,
                    createdDate = result.CreatedDate,
                    instructions = new
                    {
                        nextSteps = new[]
                        {
                            "1. Check the RFQs page at /rfqs to see the generated RFQ",
                            "2. Check the Projects page at /projects to see the generated Project",
                            "3. Both should be automatically linked to this ProductBrief"
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during ProductBrief workflow test");
                return BadRequest(new
                {
                    success = false,
                    message = "Error occurred during workflow test",
                    error = ex.Message
                });
            }
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                status = "Test API is running",
                endpoints = new[]
                {
                    "POST /api/test/workflow - Creates test ProductBrief and triggers automatic workflow"
                },
                timestamp = DateTime.UtcNow
            });
        }
    }
}
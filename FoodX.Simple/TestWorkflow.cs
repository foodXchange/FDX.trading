using FoodX.Simple.Data;
using FoodX.Simple.Models;
using FoodX.Simple.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FoodX.Simple
{
    public class TestWorkflow
    {
        public static async Task<(bool success, string message, int? productBriefId, string? rfqNumber, string? projectNumber)> RunWorkflowTestAsync(string connectionString)
        {
            try
            {
                // Setup DbContext with connection string
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseSqlServer(connectionString);

                using var context = new ApplicationDbContext(optionsBuilder.Options);

                // Setup logging
                using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                var briefLogger = loggerFactory.CreateLogger<ProductBriefService>();
                var workflowLogger = loggerFactory.CreateLogger<AutomaticWorkflowService>();

                // Create services
                var workflowService = new AutomaticWorkflowService(context, workflowLogger);
                var briefService = new ProductBriefService(context, briefLogger, workflowService);

                // Create test ProductBrief with exact data from requirements
                var testBrief = new ProductBrief
                {
                    ProductName = "Premium Organic Quinoa",
                    Category = "Grains",
                    PackageSize = "25kg bulk bags",
                    CountryOfOrigin = "Peru",
                    IsKosherCertified = true,
                    KosherOrganization = "OU Kosher",
                    StorageRequirements = "Cool, dry place",
                    BenchmarkBrandReference = "Ancient Harvest",
                    AdditionalNotes = "Testing automatic workflow - this should generate RFQ and Project automatically\nKosher Requirements:\n- Organization: OU Kosher\n- Status: Pareve",
                    CreatedBy = "test-user",
                    CreatedDate = DateTime.UtcNow,
                    Status = "Active"
                };

                Console.WriteLine($"Creating ProductBrief: {testBrief.ProductName}");

                // This should automatically create RFQ and Project via workflow
                var result = await briefService.CreateBriefAsync(testBrief);

                Console.WriteLine($"ProductBrief created with ID: {result.Id}");

                // Verify RFQ was created
                var rfq = await context.RFQs.FirstOrDefaultAsync(r => r.ProductBriefId == result.Id);
                var project = await context.Projects
                    .Include(p => p.RFQ)
                    .FirstOrDefaultAsync(p => p.RFQ != null && p.RFQ.ProductBriefId == result.Id);

                if (rfq != null && project != null)
                {
                    return (true,
                        $"SUCCESS: All 3 records created! ProductBrief ID: {result.Id}, RFQ: {rfq.RFQNumber}, Project: {project.ProjectNumber}",
                        result.Id,
                        rfq.RFQNumber,
                        project.ProjectNumber);
                }
                else
                {
                    return (false,
                        $"PARTIAL: ProductBrief created (ID: {result.Id}) but workflow failed. RFQ: {(rfq != null ? rfq.RFQNumber : "NOT CREATED")}, Project: {(project != null ? project.ProjectNumber : "NOT CREATED")}",
                        result.Id,
                        rfq?.RFQNumber,
                        project?.ProjectNumber);
                }
            }
            catch (Exception ex)
            {
                return (false, $"ERROR: {ex.Message}", null, null, null);
            }
        }
    }
}
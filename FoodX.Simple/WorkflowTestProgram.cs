using FoodX.Simple;
using Microsoft.Extensions.Configuration;

class WorkflowTestProgram
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("=== FoodX.Simple Automatic Workflow Test ===");
            Console.WriteLine("This will test the automatic creation of ProductBrief -> RFQ -> Project");
            Console.WriteLine();

            // Get connection string from appsettings.json
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("ERROR: Connection string not found in appsettings.json");
                return;
            }

            Console.WriteLine("Running workflow test...");

            var (success, message, productBriefId, rfqNumber, projectNumber) =
                await TestWorkflow.RunWorkflowTestAsync(connectionString);

            Console.WriteLine();
            Console.WriteLine("=== TEST RESULTS ===");
            Console.WriteLine($"Success: {success}");
            Console.WriteLine($"Message: {message}");

            if (productBriefId.HasValue)
            {
                Console.WriteLine($"ProductBrief ID: {productBriefId}");
            }

            if (!string.IsNullOrEmpty(rfqNumber))
            {
                Console.WriteLine($"RFQ Number: {rfqNumber}");
            }

            if (!string.IsNullOrEmpty(projectNumber))
            {
                Console.WriteLine($"Project Number: {projectNumber}");
            }

            if (success)
            {
                Console.WriteLine();
                Console.WriteLine("✅ WORKFLOW TEST PASSED!");
                Console.WriteLine("All 3 records were created successfully in a single transaction:");
                Console.WriteLine("1. ProductBrief");
                Console.WriteLine("2. RFQ (auto-generated)");
                Console.WriteLine("3. Project (auto-generated)");
                Console.WriteLine();
                Console.WriteLine("You can verify by:");
                Console.WriteLine("- Navigating to http://localhost:5237/rfqs");
                Console.WriteLine("- Navigating to http://localhost:5237/projects");
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("❌ WORKFLOW TEST FAILED!");
                Console.WriteLine("The automatic workflow did not complete as expected.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FATAL ERROR: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }
}
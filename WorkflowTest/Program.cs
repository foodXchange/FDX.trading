using Microsoft.Data.SqlClient;
using System.Text.Json;

var connectionString = "Server=tcp:fdx-sql-prod.database.windows.net,1433;Database=fdxdb;User Id=foodxapp;Password=FoodX@2024!Secure#Trading;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

Console.WriteLine("=== FoodX Automatic Workflow Test ===");
Console.WriteLine("Testing ProductBrief -> RFQ -> Project workflow");
Console.WriteLine();

try
{
    // Step 1: Create ProductBrief record
    Console.WriteLine("Step 1: Creating ProductBrief record...");

    var insertSql = @"
        INSERT INTO ProductBriefs (
            ProductName,
            Category,
            PackageSize,
            CountryOfOrigin,
            IsKosherCertified,
            KosherOrganization,
            StorageRequirements,
            BenchmarkBrandReference,
            AdditionalNotes,
            CreatedDate,
            CreatedBy,
            Status
        ) OUTPUT INSERTED.Id
        VALUES (
            @ProductName,
            @Category,
            @PackageSize,
            @CountryOfOrigin,
            @IsKosherCertified,
            @KosherOrganization,
            @StorageRequirements,
            @BenchmarkBrandReference,
            @AdditionalNotes,
            @CreatedDate,
            @CreatedBy,
            @Status
        );";

    int productBriefId;

    using (var connection = new SqlConnection(connectionString))
    {
        await connection.OpenAsync();

        using var command = new SqlCommand(insertSql, connection);
        command.Parameters.AddWithValue("@ProductName", "Premium Organic Quinoa");
        command.Parameters.AddWithValue("@Category", "Grains");
        command.Parameters.AddWithValue("@PackageSize", "25kg bulk bags");
        command.Parameters.AddWithValue("@CountryOfOrigin", "Peru");
        command.Parameters.AddWithValue("@IsKosherCertified", true);
        command.Parameters.AddWithValue("@KosherOrganization", "OU Kosher");
        command.Parameters.AddWithValue("@StorageRequirements", "Cool, dry place");
        command.Parameters.AddWithValue("@BenchmarkBrandReference", "Ancient Harvest");
        command.Parameters.AddWithValue("@AdditionalNotes", "Testing automatic workflow - this should generate RFQ and Project automatically");
        command.Parameters.AddWithValue("@CreatedDate", DateTime.UtcNow);
        command.Parameters.AddWithValue("@CreatedBy", "test-user");
        command.Parameters.AddWithValue("@Status", "Active");

        productBriefId = (int)await command.ExecuteScalarAsync();
    }

    Console.WriteLine($"✅ ProductBrief created with ID: {productBriefId}");

    // Step 2: Wait a moment for any potential triggers
    Console.WriteLine("Step 2: Waiting for potential workflow triggers...");
    await Task.Delay(2000);

    // Step 3: Check if RFQ was created
    Console.WriteLine("Step 3: Checking for auto-generated RFQ...");

    var checkRfqSql = "SELECT Id, RFQNumber, Title, Status, IssueDate FROM RFQs WHERE ProductBriefId = @ProductBriefId";

    string? rfqNumber = null;
    int? rfqId = null;

    using (var connection = new SqlConnection(connectionString))
    {
        await connection.OpenAsync();

        using var command = new SqlCommand(checkRfqSql, connection);
        command.Parameters.AddWithValue("@ProductBriefId", productBriefId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            rfqId = reader.GetInt32(0);
            rfqNumber = reader.GetString(1);
            var title = reader.GetString(2);
            var status = reader.GetString(3);
            var issueDate = reader.GetDateTime(4);

            Console.WriteLine($"✅ RFQ found! Number: {rfqNumber}, Title: {title}, Status: {status}");
        }
        else
        {
            Console.WriteLine("❌ No RFQ found");
        }
    }

    // Step 4: Check if Project was created
    Console.WriteLine("Step 4: Checking for auto-generated Project...");

    var checkProjectSql = "SELECT Id, ProjectNumber, Title, Status, StartDate FROM Projects WHERE RFQId = @RFQId";

    string? projectNumber = null;

    if (rfqId.HasValue)
    {
        using (var connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();

            using var command = new SqlCommand(checkProjectSql, connection);
            command.Parameters.AddWithValue("@RFQId", rfqId.Value);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var projectId = reader.GetInt32(0);
                projectNumber = reader.GetString(1);
                var title = reader.GetString(2);
                var status = reader.GetString(3);
                var startDate = reader.GetDateTime(4);

                Console.WriteLine($"✅ Project found! Number: {projectNumber}, Title: {title}, Status: {status}");
            }
            else
            {
                Console.WriteLine("❌ No Project found");
            }
        }
    }
    else
    {
        Console.WriteLine("❌ Cannot check for Project - no RFQ found");
    }

    // Step 5: Summary
    Console.WriteLine();
    Console.WriteLine("=== TEST RESULTS ===");

    var recordsCreated = 1; // ProductBrief always created
    if (!string.IsNullOrEmpty(rfqNumber)) recordsCreated++;
    if (!string.IsNullOrEmpty(projectNumber)) recordsCreated++;

    Console.WriteLine($"Records Created: {recordsCreated}/3");
    Console.WriteLine($"ProductBrief ID: {productBriefId}");
    Console.WriteLine($"RFQ Number: {rfqNumber ?? "NOT CREATED"}");
    Console.WriteLine($"Project Number: {projectNumber ?? "NOT CREATED"}");

    if (recordsCreated == 3)
    {
        Console.WriteLine();
        Console.WriteLine("🎉 SUCCESS: Automatic workflow completed!");
        Console.WriteLine("All 3 records were created:");
        Console.WriteLine("1. ProductBrief");
        Console.WriteLine("2. RFQ (auto-generated)");
        Console.WriteLine("3. Project (auto-generated)");
        Console.WriteLine();
        Console.WriteLine("Verification URLs:");
        Console.WriteLine("- RFQs: http://localhost:5237/rfqs");
        Console.WriteLine("- Projects: http://localhost:5237/projects");
    }
    else
    {
        Console.WriteLine();
        Console.WriteLine("⚠️ PARTIAL SUCCESS: Not all records were created automatically");
        Console.WriteLine("This suggests the automatic workflow may not be working as expected.");
        Console.WriteLine("Manual intervention may be required.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ ERROR: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
}

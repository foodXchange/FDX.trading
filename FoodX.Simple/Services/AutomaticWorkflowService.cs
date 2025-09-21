using FoodX.Simple.Data;
using FoodX.Simple.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodX.Simple.Services
{
    public class AutomaticWorkflowService : IAutomaticWorkflowService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AutomaticWorkflowService> _logger;

        public AutomaticWorkflowService(
            ApplicationDbContext context,
            ILogger<AutomaticWorkflowService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<RFQ> CreateRFQFromProductBriefAsync(int productBriefId, string userId)
        {
            _logger.LogInformation("Creating RFQ from ProductBrief {ProductBriefId}", productBriefId);

            var productBrief = await _context.ProductBriefs
                .FirstOrDefaultAsync(pb => pb.Id == productBriefId);

            if (productBrief == null)
            {
                throw new ArgumentException($"ProductBrief with ID {productBriefId} not found");
            }

            // Check if RFQ already exists
            var existingRFQ = await _context.RFQs
                .FirstOrDefaultAsync(r => r.ProductBriefId == productBriefId);

            if (existingRFQ != null)
            {
                _logger.LogInformation("RFQ already exists for ProductBrief {ProductBriefId}", productBriefId);
                return existingRFQ;
            }

            // Create new RFQ from ProductBrief data
            var rfq = new RFQ
            {
                RFQNumber = await GenerateRFQNumberAsync(),
                Title = productBrief.ProductName,
                Description = productBrief.AdditionalNotes ?? $"RFQ for {productBrief.ProductName}",
                Category = productBrief.Category,
                PackageSize = productBrief.PackageSize,
                CountryOfOrigin = productBrief.CountryOfOrigin,
                IsKosherCertified = productBrief.IsKosherCertified,
                KosherOrganization = productBrief.KosherOrganization,
                SpecialAttributes = productBrief.SpecialAttributes,
                AdditionalNotes = productBrief.AdditionalNotes,
                IssueDate = DateTime.UtcNow,
                ResponseDeadline = DateTime.UtcNow.AddDays(14), // 2 weeks default
                ProductBriefId = productBriefId,
                CreatedBy = userId,
                Status = "Active"
            };

            _context.RFQs.Add(rfq);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created RFQ {RFQNumber} for ProductBrief {ProductBriefId}", rfq.RFQNumber, productBriefId);

            return rfq;
        }

        public async Task<Project> CreateProjectFromRFQAsync(int rfqId, string userId)
        {
            _logger.LogInformation("Creating Project from RFQ {RFQId}", rfqId);

            var rfq = await _context.RFQs
                .Include(r => r.ProductBrief)
                .FirstOrDefaultAsync(r => r.Id == rfqId);

            if (rfq == null)
            {
                throw new ArgumentException($"RFQ with ID {rfqId} not found");
            }

            // Check if Project already exists
            var existingProject = await _context.Projects
                .FirstOrDefaultAsync(p => p.RFQId == rfqId);

            if (existingProject != null)
            {
                _logger.LogInformation("Project already exists for RFQ {RFQId}", rfqId);
                return existingProject;
            }

            // Create new Project from RFQ data
            var project = new Project
            {
                ProjectNumber = await GenerateProjectNumberAsync(),
                Title = $"Project: {rfq.Title}",
                Description = $"Procurement project for {rfq.Title}",
                Status = "Planning",
                Priority = "Medium",
                StartDate = DateTime.UtcNow,
                ExpectedEndDate = rfq.ResponseDeadline.AddDays(30), // 30 days after RFQ deadline
                AssignedTo = userId, // Assign to the buyer who created the brief
                RFQId = rfqId,
                CreatedBy = userId,
                Notes = $"Auto-generated project for RFQ {rfq.RFQNumber}"
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created Project {ProjectNumber} for RFQ {RFQId}", project.ProjectNumber, rfqId);

            return project;
        }

        public async Task<(RFQ rfq, Project project)> ProcessCompleteWorkflowAsync(int productBriefId, string userId)
        {
            _logger.LogInformation("Processing complete workflow for ProductBrief {ProductBriefId}", productBriefId);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Step 1: Create RFQ
                var rfq = await CreateRFQFromProductBriefAsync(productBriefId, userId);

                // Step 2: Create Project
                var project = await CreateProjectFromRFQAsync(rfq.Id, userId);

                // Step 3: Update ProductBrief status (workflow tracking temporarily disabled)
                var productBrief = await _context.ProductBriefs
                    .FirstOrDefaultAsync(pb => pb.Id == productBriefId);

                if (productBrief != null)
                {
                    // productBrief.IsWorkflowCompleted = true; // Temporarily commented out
                    // productBrief.WorkflowCompletedDate = DateTime.UtcNow; // Temporarily commented out
                    productBrief.Status = "Active"; // Update status to Active
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Completed workflow for ProductBrief {ProductBriefId}: RFQ {RFQNumber}, Project {ProjectNumber}",
                    productBriefId, rfq.RFQNumber, project.ProjectNumber);

                return (rfq, project);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing workflow for ProductBrief {ProductBriefId}", productBriefId);
                throw;
            }
        }

        public async Task<string> GenerateRFQNumberAsync()
        {
            var currentYear = DateTime.UtcNow.Year;
            var yearPrefix = currentYear.ToString();

            // Get the count of RFQs created this year
            var count = await _context.RFQs
                .Where(r => r.RFQNumber.StartsWith($"RFQ-{yearPrefix}-"))
                .CountAsync();

            var nextNumber = count + 1;
            return $"RFQ-{yearPrefix}-{nextNumber:D3}"; // Format: RFQ-2024-001
        }

        public async Task<string> GenerateProjectNumberAsync()
        {
            var currentYear = DateTime.UtcNow.Year;
            var yearPrefix = currentYear.ToString();

            // Get the count of Projects created this year
            var count = await _context.Projects
                .Where(p => p.ProjectNumber.StartsWith($"PRJ-{yearPrefix}-"))
                .CountAsync();

            var nextNumber = count + 1;
            return $"PRJ-{yearPrefix}-{nextNumber:D3}"; // Format: PRJ-2024-001
        }
    }
}
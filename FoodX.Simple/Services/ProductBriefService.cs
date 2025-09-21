using FoodX.Simple.Data;
using FoodX.Simple.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodX.Simple.Services;

public class ProductBriefService : IProductBriefService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductBriefService> _logger;
    private readonly IAutomaticWorkflowService _workflowService;

    public ProductBriefService(
        ApplicationDbContext context,
        ILogger<ProductBriefService> logger,
        IAutomaticWorkflowService workflowService)
    {
        _context = context;
        _logger = logger;
        _workflowService = workflowService;
    }

    public async Task<List<ProductBrief>> GetAllBriefsAsync()
    {
        return await _context.ProductBriefs
            .OrderByDescending(b => b.CreatedDate)
            .ToListAsync();
    }

    public async Task<ProductBrief?> GetBriefByIdAsync(int id)
    {
        return await _context.ProductBriefs.FindAsync(id);
    }

    public async Task<ProductBrief> CreateBriefAsync(ProductBrief brief)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Step 1: Save ProductBrief
            _context.ProductBriefs.Add(brief);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created product brief: {brief.ProductName}");

            // Step 2: IMMEDIATELY trigger automatic workflow (RFQ + Project creation)
            var (rfq, project) = await _workflowService.ProcessCompleteWorkflowAsync(brief.Id, brief.CreatedBy);

            _logger.LogInformation($"Automatically created RFQ {rfq.RFQNumber} and Project {project.ProjectNumber} for brief {brief.ProductName}");

            await transaction.CommitAsync();
            return brief;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, $"Error creating product brief and workflow for: {brief.ProductName}");
            throw;
        }
    }

    public async Task<ProductBrief> UpdateBriefAsync(ProductBrief brief)
    {
        _context.Update(brief);
        await _context.SaveChangesAsync();
        return brief;
    }

    public async Task DeleteBriefAsync(int id)
    {
        var brief = await _context.ProductBriefs.FindAsync(id);
        if (brief == null) return;

        _context.ProductBriefs.Remove(brief);
        await _context.SaveChangesAsync();
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        return await Task.FromResult(new List<string>
        {
            "Bakery & Bread",
            "Beverages",
            "Canned & Jarred Goods",
            "Dairy & Eggs",
            "Frozen Foods",
            "Fresh Produce",
            "Meat & Seafood",
            "Snacks & Confectionery",
            "Condiments & Sauces",
            "Grains & Pasta",
            "Health & Wellness"
        });
    }

    public async Task<List<string>> GetCountriesAsync()
    {
        return await Task.FromResult(new List<string>
        {
            "United States",
            "United Kingdom",
            "Germany",
            "France",
            "Italy",
            "Spain",
            "Netherlands",
            "Belgium",
            "Poland",
            "Turkey",
            "China",
            "India",
            "Thailand",
            "Vietnam"
        });
    }
}
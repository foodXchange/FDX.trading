using FoodX.Simple.Models;

namespace FoodX.Simple.Services
{
    public interface IAutomaticWorkflowService
    {
        Task<RFQ> CreateRFQFromProductBriefAsync(int productBriefId, string userId);
        Task<Project> CreateProjectFromRFQAsync(int rfqId, string userId);
        Task<(RFQ rfq, Project project)> ProcessCompleteWorkflowAsync(int productBriefId, string userId);
        Task<string> GenerateRFQNumberAsync();
        Task<string> GenerateProjectNumberAsync();
    }
}
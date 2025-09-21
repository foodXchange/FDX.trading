using FoodX.Simple.Models;

namespace FoodX.Simple.Services
{
    public interface IProductBriefService
    {
        Task<List<ProductBrief>> GetAllBriefsAsync();
        Task<ProductBrief?> GetBriefByIdAsync(int id);
        Task<ProductBrief> CreateBriefAsync(ProductBrief brief);
        Task<ProductBrief> UpdateBriefAsync(ProductBrief brief);
        Task DeleteBriefAsync(int id);
        Task<List<string>> GetCategoriesAsync();
        Task<List<string>> GetCountriesAsync();
    }
}
using FoodX.Simple.Application.DTOs;
using FoodX.Simple.Domain.Common;

namespace FoodX.Simple.Application.Interfaces
{
    public interface IProductBriefAppService
    {
        Task<Result<ProductBriefDto>> GetByIdAsync(int id);
        Task<Result<IEnumerable<ProductBriefDto>>> GetAllAsync();
        Task<Result<IEnumerable<ProductBriefDto>>> GetByUserAsync(string userId);
        Task<Result<ProductBriefDto>> CreateAsync(CreateProductBriefDto dto, string userId);
        Task<Result<ProductBriefDto>> UpdateAsync(UpdateProductBriefDto dto, string userId);
        Task<Result> DeleteAsync(int id);
        Task<Result<IEnumerable<string>>> GetCategoriesAsync();
        Task<Result<IEnumerable<string>>> GetCountriesAsync();
    }
}
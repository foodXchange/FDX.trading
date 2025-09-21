using Microsoft.Extensions.Caching.Memory;
using FoodX.Simple.Application.DTOs;
using FoodX.Simple.Application.Interfaces;
using FoodX.Simple.Domain.Common;

namespace FoodX.Simple.Infrastructure.Services
{
    public class CachedProductBriefService : IProductBriefAppService
    {
        private readonly IProductBriefAppService _inner;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachedProductBriefService> _logger;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

        public CachedProductBriefService(
            IProductBriefAppService inner,
            IMemoryCache cache,
            ILogger<CachedProductBriefService> logger)
        {
            _inner = inner;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Result<ProductBriefDto>> GetByIdAsync(int id)
        {
            var cacheKey = $"productbrief_{id}";

            if (_cache.TryGetValue<ProductBriefDto>(cacheKey, out var cached))
            {
                _logger.LogDebug("Cache hit for ProductBrief {Id}", id);
                return Result.Success(cached);
            }

            var result = await _inner.GetByIdAsync(id);

            if (result.IsSuccess && result.Value != null)
            {
                _cache.Set(cacheKey, result.Value, _cacheExpiration);
                _logger.LogDebug("Cached ProductBrief {Id}", id);
            }

            return result;
        }

        public async Task<Result<IEnumerable<ProductBriefDto>>> GetAllAsync()
        {
            const string cacheKey = "productbriefs_all";

            if (_cache.TryGetValue<IEnumerable<ProductBriefDto>>(cacheKey, out var cached))
            {
                _logger.LogDebug("Cache hit for all ProductBriefs");
                return Result.Success(cached);
            }

            var result = await _inner.GetAllAsync();

            if (result.IsSuccess)
            {
                _cache.Set(cacheKey, result.Value, TimeSpan.FromMinutes(1)); // Shorter expiry for list
                _logger.LogDebug("Cached all ProductBriefs");
            }

            return result;
        }

        public async Task<Result<IEnumerable<ProductBriefDto>>> GetByUserAsync(string userId)
        {
            var cacheKey = $"productbriefs_user_{userId}";

            if (_cache.TryGetValue<IEnumerable<ProductBriefDto>>(cacheKey, out var cached))
            {
                _logger.LogDebug("Cache hit for user {UserId} ProductBriefs", userId);
                return Result.Success(cached);
            }

            var result = await _inner.GetByUserAsync(userId);

            if (result.IsSuccess)
            {
                _cache.Set(cacheKey, result.Value, TimeSpan.FromMinutes(2));
                _logger.LogDebug("Cached user {UserId} ProductBriefs", userId);
            }

            return result;
        }

        public async Task<Result<ProductBriefDto>> CreateAsync(CreateProductBriefDto dto, string userId)
        {
            var result = await _inner.CreateAsync(dto, userId);

            if (result.IsSuccess)
            {
                // Invalidate related caches
                _cache.Remove("productbriefs_all");
                _cache.Remove($"productbriefs_user_{userId}");
                _logger.LogDebug("Invalidated caches after creating ProductBrief");
            }

            return result;
        }

        public async Task<Result<ProductBriefDto>> UpdateAsync(UpdateProductBriefDto dto, string userId)
        {
            var result = await _inner.UpdateAsync(dto, userId);

            if (result.IsSuccess)
            {
                // Invalidate related caches
                _cache.Remove($"productbrief_{dto.Id}");
                _cache.Remove("productbriefs_all");
                _cache.Remove($"productbriefs_user_{userId}");
                _logger.LogDebug("Invalidated caches after updating ProductBrief {Id}", dto.Id);
            }

            return result;
        }

        public async Task<Result> DeleteAsync(int id)
        {
            var result = await _inner.DeleteAsync(id);

            if (result.IsSuccess)
            {
                // Invalidate related caches
                _cache.Remove($"productbrief_{id}");
                _cache.Remove("productbriefs_all");
                _logger.LogDebug("Invalidated caches after deleting ProductBrief {Id}", id);
            }

            return result;
        }

        public async Task<Result<IEnumerable<string>>> GetCategoriesAsync()
        {
            const string cacheKey = "categories";

            if (_cache.TryGetValue<IEnumerable<string>>(cacheKey, out var cached))
            {
                return Result.Success(cached);
            }

            var result = await _inner.GetCategoriesAsync();

            if (result.IsSuccess)
            {
                _cache.Set(cacheKey, result.Value, TimeSpan.FromHours(1));
            }

            return result;
        }

        public async Task<Result<IEnumerable<string>>> GetCountriesAsync()
        {
            const string cacheKey = "countries";

            if (_cache.TryGetValue<IEnumerable<string>>(cacheKey, out var cached))
            {
                return Result.Success(cached);
            }

            var result = await _inner.GetCountriesAsync();

            if (result.IsSuccess)
            {
                _cache.Set(cacheKey, result.Value, TimeSpan.FromHours(1));
            }

            return result;
        }
    }
}
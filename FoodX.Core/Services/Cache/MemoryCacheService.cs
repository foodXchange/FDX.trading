using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FoodX.Core.Services.Cache;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public T? Get<T>(string key)
    {
        _cache.TryGetValue(key, out T? value);
        return value;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        return Task.FromResult(Get<T>(key));
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var cacheOptions = new MemoryCacheEntryOptions();
        
        if (expiration.HasValue)
        {
            cacheOptions.SetAbsoluteExpiration(expiration.Value);
        }
        else
        {
            cacheOptions.SetSlidingExpiration(TimeSpan.FromMinutes(30));
        }

        _cache.Set(key, value, cacheOptions);
        _logger.LogDebug("Cache set for key: {Key}", key);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        Set(key, value, expiration);
        return Task.CompletedTask;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        _logger.LogDebug("Cache removed for key: {Key}", key);
    }

    public Task RemoveAsync(string key)
    {
        Remove(key);
        return Task.CompletedTask;
    }

    public bool TryGetValue<T>(string key, out T? value)
    {
        return _cache.TryGetValue(key, out value);
    }
}
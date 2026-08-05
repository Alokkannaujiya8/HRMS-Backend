using HRMS.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HRMS.Infrastructure.Services
{
    /// <summary>
    /// Distributed caching service implementation wrapping IDistributedCache with System.Text.Json serialization.
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RedisCacheService> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var cachedData = await _cache.GetStringAsync(key, cancellationToken);
                if (string.IsNullOrWhiteSpace(cachedData))
                {
                    _logger.LogInformation("Cache MISS for key: {CacheKey}", key);
                    return default;
                }

                _logger.LogInformation("Cache HIT for key: {CacheKey}", key);
                return JsonSerializer.Deserialize<T>(cachedData, JsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read key {CacheKey} from distributed cache", key);
                return default;
            }
        }

        public async Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default)
        {
            if (value is null)
            {
                return;
            }

            try
            {
                var serialized = JsonSerializer.Serialize(value, JsonOptions);
                var options = new DistributedCacheEntryOptions();

                if (absoluteExpiration.HasValue)
                {
                    options.SetAbsoluteExpiration(absoluteExpiration.Value);
                }
                else
                {
                    options.SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                }

                if (slidingExpiration.HasValue)
                {
                    options.SetSlidingExpiration(slidingExpiration.Value);
                }

                await _cache.SetStringAsync(key, serialized, options, cancellationToken);
                _logger.LogInformation("Cache SET for key: {CacheKey}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set key {CacheKey} in distributed cache", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _cache.RemoveAsync(key, cancellationToken);
                _logger.LogInformation("Cache INVALIDATED/REMOVED for key: {CacheKey}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove key {CacheKey} from distributed cache", key);
            }
        }
    }
}

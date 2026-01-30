using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Products.Application.Interfaces;
using StackExchange.Redis;

namespace Products.Infrastructure.Caching;

/// <summary>
/// Redis tabanlý cache servisi implementasyonu
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(
        IDistributedCache cache,
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _redis = redis;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var cachedData = await _cache.GetStringAsync(key, cancellationToken);
            
            if (string.IsNullOrEmpty(cachedData))
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(cachedData, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting cache for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(15)
            };

            var serializedData = JsonSerializer.Serialize(value, _jsonOptions);
            await _cache.SetStringAsync(key, serializedData, options, cancellationToken);
            
            _logger.LogDebug("Cache set for key: {Key} with expiration: {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error setting cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Cache removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing cache for key: {Key}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: $"{prefix}:*").ToArray();

            if (keys.Length > 0)
            {
                var db = _redis.GetDatabase();
                await db.KeyDeleteAsync(keys);
                _logger.LogInformation("Removed {Count} cache entries with prefix: {Prefix}", keys.Length, prefix);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing cache by prefix: {Prefix}", prefix);
        }
    }
}

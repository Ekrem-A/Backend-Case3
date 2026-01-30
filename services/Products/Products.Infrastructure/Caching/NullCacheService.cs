using Microsoft.Extensions.Logging;
using Products.Application.Interfaces;

namespace Products.Infrastructure.Caching;

/// <summary>
/// Redis baðlantýsý olmadýðýnda kullanýlacak null cache servisi
/// Development ve test ortamlarýnda kullanýþlý
/// </summary>
public class NullCacheService : ICacheService
{
    private readonly ILogger<NullCacheService> _logger;

    public NullCacheService(ILogger<NullCacheService> logger)
    {
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        _logger.LogDebug("[NullCacheService] Get called for key: {Key}", key);
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        _logger.LogDebug("[NullCacheService] Set called for key: {Key}", key);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("[NullCacheService] Remove called for key: {Key}", key);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("[NullCacheService] RemoveByPrefix called for prefix: {Prefix}", prefix);
        return Task.CompletedTask;
    }
}

namespace Products.Application.Interfaces;

/// <summary>
/// Cache servisi için interface
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Cache'den veri getirir
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Cache'e veri ekler
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Cache'den veri siler
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirli bir pattern'e uyan tüm cache'leri siler
    /// </summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}

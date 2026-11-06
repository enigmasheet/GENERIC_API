namespace ModularMonolith.Infrastructure.Caching;

public readonly record struct CacheResult<T>(bool Found, T? Value)
{
    public static CacheResult<T> Miss => new(false, default);

    public static CacheResult<T> Hit(T value) => new(true, value);
}

public interface ICacheService
{
    Task<CacheResult<T>> GetAsync<T>(
        string group,
        string key,
        CancellationToken cancellationToken = default);

    Task SetAsync<T>(
        string group,
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        string group,
        string key,
        CancellationToken cancellationToken = default);

    Task InvalidateGroupAsync(
        string group,
        CancellationToken cancellationToken = default);

    Task<T> GetOrCreateAsync<T>(
        string group,
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);
}

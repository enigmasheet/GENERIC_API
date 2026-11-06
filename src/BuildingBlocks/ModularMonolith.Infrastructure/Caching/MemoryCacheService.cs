using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ModularMonolith.Infrastructure.Options;

namespace ModularMonolith.Infrastructure.Caching;

public sealed class MemoryCacheService(
    IMemoryCache memoryCache,
    IOptions<RedisOptions> options) : ICacheService
{
    private readonly ConcurrentDictionary<string, long> _groupVersions = new(StringComparer.Ordinal);

    public Task<CacheResult<T>> GetAsync<T>(
        string group,
        string key,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildKey(group, key);

        if (memoryCache.TryGetValue(cacheKey, out T? value))
        {
            return Task.FromResult(CacheResult<T>.Hit(value!));
        }

        return Task.FromResult(CacheResult<T>.Miss);
    }

    public Task SetAsync<T>(
        string group,
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        memoryCache.Set(
            BuildKey(group, key),
            value,
            expiration ?? TimeSpan.FromSeconds(options.Value.DefaultTtlSeconds));

        return Task.CompletedTask;
    }

    public Task RemoveAsync(
        string group,
        string key,
        CancellationToken cancellationToken = default)
    {
        memoryCache.Remove(BuildKey(group, key));

        return Task.CompletedTask;
    }

    public Task InvalidateGroupAsync(
        string group,
        CancellationToken cancellationToken = default)
    {
        _groupVersions.AddOrUpdate(group, 1, (_, current) => current + 1);

        return Task.CompletedTask;
    }

    public async Task<T> GetOrCreateAsync<T>(
        string group,
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<T>(group, key, cancellationToken);

        if (result.Found)
        {
            return result.Value!;
        }

        var created = await factory(cancellationToken);
        await SetAsync(group, key, created, expiration, cancellationToken);

        return created;
    }

    private string BuildKey(string group, string key)
    {
        var version = _groupVersions.GetValueOrDefault(group, 1);

        return $"{options.Value.InstanceName}cache:{group}:v{version}:{key}";
    }
}

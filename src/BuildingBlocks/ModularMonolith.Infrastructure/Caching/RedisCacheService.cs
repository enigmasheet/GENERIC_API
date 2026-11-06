using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularMonolith.Infrastructure.Options;
using StackExchange.Redis;

namespace ModularMonolith.Infrastructure.Caching;

public sealed class RedisCacheService(
    IDistributedCache distributedCache,
    IConnectionMultiplexer connectionMultiplexer,
    IOptions<RedisOptions> options,
    ILogger<RedisCacheService> logger) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<CacheResult<T>> GetAsync<T>(
        string group,
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = await BuildKeyAsync(group, key, cancellationToken);
            var payload = await distributedCache.GetAsync(cacheKey, cancellationToken);

            if (payload is null)
            {
                return CacheResult<T>.Miss;
            }

            var value = JsonSerializer.Deserialize<T>(payload, JsonOptions);

            return value is null
                ? CacheResult<T>.Miss
                : CacheResult<T>.Hit(value);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Redis cache read failed for {Group}/{Key}.", group, key);
            return CacheResult<T>.Miss;
        }
    }

    public async Task SetAsync<T>(
        string group,
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = await BuildKeyAsync(group, key, cancellationToken);
            var payload = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);

            await distributedCache.SetAsync(
                cacheKey,
                payload,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        expiration ?? TimeSpan.FromSeconds(options.Value.DefaultTtlSeconds)
                },
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Redis cache write failed for {Group}/{Key}.", group, key);
        }
    }

    public async Task RemoveAsync(
        string group,
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = await BuildKeyAsync(group, key, cancellationToken);
            await distributedCache.RemoveAsync(cacheKey, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Redis cache removal failed for {Group}/{Key}.", group, key);
        }
    }

    public async Task InvalidateGroupAsync(
        string group,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await connectionMultiplexer.GetDatabase().StringIncrementAsync(VersionKey(group));
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Redis cache group invalidation failed for {Group}.", group);
        }
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

    private async Task<string> BuildKeyAsync(
        string group,
        string key,
        CancellationToken cancellationToken)
    {
        var version = await connectionMultiplexer.GetDatabase().StringGetAsync(VersionKey(group));
        var groupVersion = version.HasValue ? version.ToString() : "1";

        return $"{options.Value.InstanceName}cache:{group}:v{groupVersion}:{key}";
    }

    private string VersionKey(string group)
    {
        return $"{options.Value.InstanceName}cache-version:{group}";
    }
}

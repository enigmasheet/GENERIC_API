using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularMonolith.Infrastructure.Options;
using StackExchange.Redis;

namespace ModularMonolith.Infrastructure.Caching;

public sealed class RedisDistributedLockService(
    IConnectionMultiplexer connectionMultiplexer,
    IOptions<RedisOptions> options,
    ILogger<RedisDistributedLockService> logger) : IDistributedLockService
{
    private const string ReleaseScript = """
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('del', KEYS[1])
        else
            return 0
        end
        """;

    public async Task<IAsyncDisposable?> TryAcquireAsync(
        string key,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        var token = Guid.NewGuid().ToString("N");
        var redisKey = BuildKey(key);

        try
        {
            var acquired = await connectionMultiplexer.GetDatabase().StringSetAsync(
                redisKey,
                token,
                expiry ?? TimeSpan.FromSeconds(options.Value.LockTtlSeconds),
                When.NotExists);

            return acquired
                ? new Handle(connectionMultiplexer, redisKey, token, logger)
                : null;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Redis lock acquisition failed for {LockKey}.", key);
            return null;
        }
    }

    private string BuildKey(string key)
    {
        return $"{options.Value.InstanceName}lock:{key}";
    }

    private sealed class Handle(
        IConnectionMultiplexer connectionMultiplexer,
        RedisKey key,
        RedisValue token,
        ILogger logger) : IAsyncDisposable
    {
        private bool _released;

        public async ValueTask DisposeAsync()
        {
            if (_released)
            {
                return;
            }

            _released = true;

            try
            {
                await connectionMultiplexer.GetDatabase().ScriptEvaluateAsync(
                    ReleaseScript,
                    [key],
                    [token]);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Redis lock release failed for {LockKey}.", key);
            }
        }
    }
}

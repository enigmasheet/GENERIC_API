using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using ModularMonolith.Infrastructure.Options;

namespace ModularMonolith.Infrastructure.Caching;

public sealed class MemoryDistributedLockService(IOptions<RedisOptions> options) : IDistributedLockService
{
    private readonly ConcurrentDictionary<string, LockEntry> _locks = new(StringComparer.Ordinal);

    public Task<IAsyncDisposable?> TryAcquireAsync(
        string key,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        var token = Guid.NewGuid().ToString("N");
        var entry = new LockEntry(token, DateTimeOffset.UtcNow.Add(expiry ?? TimeSpan.FromSeconds(options.Value.LockTtlSeconds)));

        while (true)
        {
            if (_locks.TryAdd(key, entry))
            {
                return Task.FromResult<IAsyncDisposable?>(new Handle(this, key, token));
            }

            if (!_locks.TryGetValue(key, out var existing))
            {
                continue;
            }

            if (existing.ExpiresAt > DateTimeOffset.UtcNow)
            {
                return Task.FromResult<IAsyncDisposable?>(null);
            }

            _locks.TryUpdate(key, entry, existing);
        }
    }

    private void Release(string key, string token)
    {
        if (_locks.TryGetValue(key, out var existing) && existing.Token == token)
        {
            _locks.TryRemove(key, out _);
        }
    }

    private sealed record LockEntry(string Token, DateTimeOffset ExpiresAt);

    private sealed class Handle(
        MemoryDistributedLockService owner,
        string key,
        string token) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            owner.Release(key, token);
            return ValueTask.CompletedTask;
        }
    }
}

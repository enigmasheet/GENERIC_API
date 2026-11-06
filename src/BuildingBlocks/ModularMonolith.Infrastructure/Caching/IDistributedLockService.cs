namespace ModularMonolith.Infrastructure.Caching;

public interface IDistributedLockService
{
    Task<IAsyncDisposable?> TryAcquireAsync(
        string key,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default);
}

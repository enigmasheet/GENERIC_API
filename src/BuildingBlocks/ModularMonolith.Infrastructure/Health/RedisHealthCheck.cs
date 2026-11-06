using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace ModularMonolith.Infrastructure.Health;

public sealed class RedisHealthCheck(IConnectionMultiplexer connectionMultiplexer) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var latency = await connectionMultiplexer.GetDatabase().PingAsync();

            return HealthCheckResult.Healthy($"Redis responded in {latency.TotalMilliseconds:F0} ms.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Redis is not reachable.", exception);
        }
    }
}

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ModularMonolith.Infrastructure.Health;

public sealed class SelfHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy("Application process is running."));
    }
}

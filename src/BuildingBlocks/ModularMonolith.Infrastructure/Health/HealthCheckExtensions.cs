using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Infrastructure.Options;

namespace ModularMonolith.Infrastructure.Health;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddPlatformHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisOptions = configuration
            .GetSection(RedisOptions.SectionName)
            .Get<RedisOptions>() ?? new RedisOptions();

        var builder = services
            .AddHealthChecks()
            .AddCheck<SelfHealthCheck>("self", tags: ["live", "ready"])
            .AddCheck<PostgresHealthCheck>("postgres", tags: ["ready"]);

        if (redisOptions.Enabled)
        {
            builder.AddCheck<RedisHealthCheck>("redis", tags: ["ready"]);
        }

        return services;
    }

    public static IEndpointRouteBuilder MapPlatformHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteAsync,
            Predicate = _ => true
        }).AllowAnonymous().DisableRateLimiting();

        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteAsync,
            Predicate = registration => registration.Tags.Contains("live")
        }).AllowAnonymous().DisableRateLimiting();

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteAsync,
            Predicate = registration => registration.Tags.Contains("ready")
        }).AllowAnonymous().DisableRateLimiting();

        return endpoints;
    }
}

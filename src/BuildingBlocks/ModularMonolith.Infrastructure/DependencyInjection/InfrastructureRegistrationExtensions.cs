using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ModularMonolith.Infrastructure.Caching;
using ModularMonolith.Infrastructure.Correlation;
using ModularMonolith.Infrastructure.Errors;
using ModularMonolith.Infrastructure.Health;
using ModularMonolith.Infrastructure.Identity;
using ModularMonolith.Infrastructure.Observability;
using ModularMonolith.Infrastructure.Options;
using ModularMonolith.Infrastructure.Persistence.Outbox;
using ModularMonolith.Infrastructure.Security;
using ModularMonolith.Infrastructure.Tenancy;
using ModularMonolith.SharedKernel.Abstractions;
using StackExchange.Redis;

namespace ModularMonolith.Infrastructure.DependencyInjection;

public static class InfrastructureRegistrationExtensions
{
    public static IServiceCollection AddPlatformInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddPlatformOptions(configuration);
        services.AddHttpContextAccessor();
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        services.AddScoped<ICorrelationContext, HttpContextCorrelationContext>();
        services.AddSingleton<ITenantContext, ConfiguredTenantContext>();

        services.AddPlatformCaching(configuration);
        services.AddPlatformOutbox();
        services.AddPlatformHealthChecks(configuration);
        services.AddPlatformObservability(configuration);
        services.AddPlatformProblemDetails();
        services.AddPlatformCors(configuration);
        services.AddPlatformRateLimiting(configuration);

        return services;
    }

    public static IServiceCollection AddPlatformCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisOptions = configuration
            .GetSection(RedisOptions.SectionName)
            .Get<RedisOptions>() ?? new RedisOptions();

        if (redisOptions.Enabled)
        {
            services.TryAddSingleton<IConnectionMultiplexer>(_ =>
            {
                var redisConfiguration = ConfigurationOptions.Parse(redisOptions.Configuration);
                redisConfiguration.AbortOnConnectFail = redisOptions.AbortOnConnectFail;
                redisConfiguration.ConnectRetry = 3;

                return ConnectionMultiplexer.Connect(redisConfiguration);
            });

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisOptions.Configuration;
                options.InstanceName = string.Empty;
            });

            services.AddSingleton<ICacheService, RedisCacheService>();
            services.AddSingleton<IDistributedLockService, RedisDistributedLockService>();

            return services;
        }

        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddSingleton<IDistributedLockService, MemoryDistributedLockService>();

        return services;
    }

    public static IServiceCollection AddPlatformOutbox(this IServiceCollection services)
    {
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}

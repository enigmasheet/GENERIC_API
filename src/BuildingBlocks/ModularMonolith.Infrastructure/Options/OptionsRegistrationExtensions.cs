using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Infrastructure.Options;

public static class OptionsRegistrationExtensions
{
    public static IServiceCollection AddPlatformOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<ApplicationOptions>()
            .BindConfiguration(ApplicationOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<DatabaseOptions>()
            .BindConfiguration(DatabaseOptions.SectionName)
            .ValidateDataAnnotations()
            .Validate(
                options => options.ConnectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase)
                    || options.ConnectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase),
                "Database connection string must contain a host definition.")
            .ValidateOnStart();

        services.AddOptions<RedisOptions>()
            .BindConfiguration(RedisOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<CorsOptions>()
            .BindConfiguration(CorsOptions.SectionName)
            .Validate(
                options => !options.Enabled || options.AllowedOrigins.Length > 0,
                "At least one CORS origin must be configured when CORS is enabled.")
            .Validate(
                options => !options.AllowCredentials
                    || options.AllowedOrigins.All(origin => origin != "*"),
                "CORS cannot allow credentials together with a wildcard origin.")
            .ValidateOnStart();

        services.AddOptions<RateLimitingOptions>()
            .BindConfiguration(RateLimitingOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<OpenTelemetryOptions>()
            .BindConfiguration(OpenTelemetryOptions.SectionName)
            .ValidateDataAnnotations()
            .Validate(
                options => Uri.TryCreate(options.OtlpEndpoint, UriKind.Absolute, out _),
                "OpenTelemetry OTLP endpoint must be an absolute URI.")
            .ValidateOnStart();

        services.AddOptions<OutboxOptions>()
            .BindConfiguration(OutboxOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<MultiTenancyOptions>()
            .BindConfiguration(MultiTenancyOptions.SectionName)
            .ValidateDataAnnotations()
            .Validate(
                options => !options.Enabled || options.DefaultTenantId != Guid.Empty,
                "A default tenant identifier is required when multi-tenancy is enabled.")
            .ValidateOnStart();

        return services;
    }
}

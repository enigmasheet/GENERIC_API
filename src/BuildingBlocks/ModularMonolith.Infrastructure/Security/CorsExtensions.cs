using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Infrastructure.Options;

namespace ModularMonolith.Infrastructure.Security;

public static class CorsExtensions
{
    public const string PolicyName = "DefaultCors";

    public static IServiceCollection AddPlatformCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(CorsOptions.SectionName)
            .Get<CorsOptions>() ?? new CorsOptions();

        if (!options.Enabled)
        {
            return services;
        }

        services.AddCors(cors =>
        {
            cors.AddPolicy(PolicyName, policy =>
            {
                policy
                    .WithOrigins(options.AllowedOrigins)
                    .WithHeaders(options.AllowedHeaders)
                    .WithMethods(options.AllowedMethods);

                if (options.AllowCredentials)
                {
                    policy.AllowCredentials();
                }
            });
        });

        return services;
    }
}

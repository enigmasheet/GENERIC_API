using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Infrastructure.Options;
using ModularMonolith.SharedKernel.Abstractions;

namespace ModularMonolith.Infrastructure.Security;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddPlatformRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(RateLimitingOptions.SectionName)
            .Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        if (!options.Enabled)
        {
            return services;
        }

        services.AddRateLimiter(rateLimiter =>
        {
            rateLimiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            rateLimiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var partitionKey = context.User.Identity?.IsAuthenticated == true
                    ? context.User.FindFirst(PlatformClaims.Subject)?.Value ?? "authenticated"
                    : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = options.PermitLimit,
                        Window = TimeSpan.FromSeconds(options.WindowSeconds),
                        QueueLimit = options.QueueLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        AutoReplenishment = true
                    });
            });

            rateLimiter.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/problem+json";

                await context.HttpContext.Response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Too many requests",
                        Detail = "The request rate limit was exceeded. Retry later.",
                        Type = "https://httpstatuses.com/429"
                    },
                    cancellationToken);
            };
        });

        return services;
    }
}

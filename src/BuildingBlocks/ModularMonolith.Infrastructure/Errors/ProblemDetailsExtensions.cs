using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Infrastructure.Correlation;

namespace ModularMonolith.Infrastructure.Errors;

public static class ProblemDetailsExtensions
{
    public static IServiceCollection AddPlatformProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["traceId"] =
                    Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier;

                context.ProblemDetails.Extensions["correlationId"] =
                    context.HttpContext.Items[CorrelationIdMiddleware.ItemKey] as string;

                context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
            };
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }
}

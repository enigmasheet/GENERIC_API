using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace ModularMonolith.Infrastructure.Correlation;

public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string HeaderName = "X-Correlation-Id";

    public const string ItemKey = "__correlation_id";

    private const int MaxCorrelationIdLength = 128;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context.Request.Headers[HeaderName]);

        context.Items[ItemKey] = correlationId;
        context.TraceIdentifier = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId,
            ["TraceId"] = Activity.Current?.TraceId.ToString()
        }))
        {
            await next(context);
        }
    }

    private static string ResolveCorrelationId(StringValues values)
    {
        var candidate = values.FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(candidate)
            && candidate.Length <= MaxCorrelationIdLength
            && candidate.All(IsAllowedCharacter))
        {
            return candidate;
        }

        return Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
    }

    private static bool IsAllowedCharacter(char character)
    {
        return char.IsLetterOrDigit(character)
            || character is '-' or '_' or '.' or ':';
    }
}

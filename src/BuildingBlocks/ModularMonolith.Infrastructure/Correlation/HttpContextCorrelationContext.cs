using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using ModularMonolith.SharedKernel.Abstractions;

namespace ModularMonolith.Infrastructure.Correlation;

public sealed class HttpContextCorrelationContext(IHttpContextAccessor httpContextAccessor) : ICorrelationContext
{
    public string CorrelationId =>
        httpContextAccessor.HttpContext?.Items[CorrelationIdMiddleware.ItemKey] as string
        ?? Activity.Current?.TraceId.ToString()
        ?? "unknown";

    public string? TraceId => Activity.Current?.TraceId.ToString();
}

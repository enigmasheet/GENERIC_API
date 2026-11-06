using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ModularMonolith.Infrastructure.Health;

public static class HealthCheckResponseWriter
{
    public static async Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var response = new
        {
            status = report.Status.ToString(),
            totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 2),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 2),
                description = entry.Value.Status == HealthStatus.Healthy
                    ? null
                    : entry.Value.Description
            }),
            timestamp = DateTimeOffset.UtcNow
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}

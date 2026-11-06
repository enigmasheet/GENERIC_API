using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Infrastructure.Options;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ModularMonolith.Infrastructure.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddPlatformObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(OpenTelemetryOptions.SectionName)
            .Get<OpenTelemetryOptions>() ?? new OpenTelemetryOptions();

        if (!options.Enabled)
        {
            return services;
        }

        var protocol = options.Protocol == "http/protobuf"
            ? OtlpExportProtocol.HttpProtobuf
            : OtlpExportProtocol.Grpc;

        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: options.ServiceName,
                serviceVersion: options.ServiceVersion))
            .WithTracing(tracing => tracing
                .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(options.SamplingRatio)))
                .AddAspNetCoreInstrumentation(aspNetCore =>
                {
                    aspNetCore.RecordException = true;
                    aspNetCore.Filter = context =>
                        !context.Request.Path.StartsWithSegments("/health");
                })
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = new Uri(options.OtlpEndpoint);
                    exporter.Protocol = protocol;
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = new Uri(options.OtlpEndpoint);
                    exporter.Protocol = protocol;
                }));

        return services;
    }
}

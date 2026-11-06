using System.ComponentModel.DataAnnotations;

namespace ModularMonolith.Infrastructure.Options;

public sealed class OpenTelemetryOptions
{
    public const string SectionName = "OpenTelemetry";

    public bool Enabled { get; set; } = true;

    [Required]
    [MinLength(1)]
    public string ServiceName { get; set; } = "ModularMonolith.Api";

    [Required]
    [MinLength(1)]
    public string ServiceVersion { get; set; } = "1.0.0";

    [Required]
    [MinLength(1)]
    public string OtlpEndpoint { get; set; } = "http://localhost:4317";

    [Required]
    [RegularExpression("^(grpc|http/protobuf)$")]
    public string Protocol { get; set; } = "grpc";

    [Range(0.0, 1.0)]
    public double SamplingRatio { get; set; } = 1.0;
}

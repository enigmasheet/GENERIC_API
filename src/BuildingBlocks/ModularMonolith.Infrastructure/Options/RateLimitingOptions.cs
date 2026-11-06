using System.ComponentModel.DataAnnotations;

namespace ModularMonolith.Infrastructure.Options;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public bool Enabled { get; set; } = true;

    [Range(1, 100000)]
    public int PermitLimit { get; set; } = 100;

    [Range(1, 86400)]
    public int WindowSeconds { get; set; } = 60;

    [Range(0, 10000)]
    public int QueueLimit { get; set; }

    public bool ApplyToHealthChecks { get; set; }
}

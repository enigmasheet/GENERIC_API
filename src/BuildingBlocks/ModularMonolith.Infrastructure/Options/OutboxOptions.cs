using System.ComponentModel.DataAnnotations;

namespace ModularMonolith.Infrastructure.Options;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public bool Enabled { get; set; } = true;

    [Range(1, 1000)]
    public int BatchSize { get; set; } = 50;

    [Range(1, 3600)]
    public int PollIntervalSeconds { get; set; } = 5;

    [Range(1, 100)]
    public int MaxAttempts { get; set; } = 10;

    [Range(1, 3650)]
    public int RetentionDays { get; set; } = 30;

    [Range(1, 1440)]
    public int CleanupIntervalMinutes { get; set; } = 60;
}

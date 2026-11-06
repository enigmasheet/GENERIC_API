using System.ComponentModel.DataAnnotations;

namespace ModularMonolith.Infrastructure.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required]
    [MinLength(1)]
    public string ConnectionString { get; set; } = string.Empty;

    public bool AutoMigrate { get; set; }

    [Range(1, 300)]
    public int CommandTimeoutSeconds { get; set; } = 30;

    [Range(0, 10)]
    public int MaxRetryCount { get; set; } = 3;

    public bool EnableDetailedErrors { get; set; }

    public bool EnableSensitiveDataLogging { get; set; }
}

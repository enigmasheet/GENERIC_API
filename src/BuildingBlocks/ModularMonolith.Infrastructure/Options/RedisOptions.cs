using System.ComponentModel.DataAnnotations;

namespace ModularMonolith.Infrastructure.Options;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public bool Enabled { get; set; } = true;

    [Required]
    [MinLength(1)]
    public string Configuration { get; set; } = "localhost:6379";

    [Required]
    [MinLength(1)]
    public string InstanceName { get; set; } = "modular:";

    [Range(1, 86400)]
    public int DefaultTtlSeconds { get; set; } = 300;

    [Range(1, 300)]
    public int LockTtlSeconds { get; set; } = 30;

    public bool AbortOnConnectFail { get; set; }
}

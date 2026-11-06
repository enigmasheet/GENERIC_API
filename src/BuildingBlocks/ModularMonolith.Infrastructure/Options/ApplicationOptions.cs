using System.ComponentModel.DataAnnotations;

namespace ModularMonolith.Infrastructure.Options;

public sealed class ApplicationOptions
{
    public const string SectionName = "Application";

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "ModularMonolith";

    [Required]
    [MaxLength(50)]
    public string Environment { get; set; } = "Development";

    [Required]
    [MaxLength(50)]
    public string Version { get; set; } = "1.0.0";
}

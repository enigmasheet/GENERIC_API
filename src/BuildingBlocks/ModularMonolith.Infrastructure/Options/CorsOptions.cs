using System.ComponentModel.DataAnnotations;

namespace ModularMonolith.Infrastructure.Options;

public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    public bool Enabled { get; set; } = true;

    public string[] AllowedOrigins { get; set; } = [];

    public string[] AllowedHeaders { get; set; } = ["*"];

    public string[] AllowedMethods { get; set; } = ["GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"];

    public bool AllowCredentials { get; set; }
}

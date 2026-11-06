using System.ComponentModel.DataAnnotations;

namespace ModularMonolith.Infrastructure.Options;

public sealed class MultiTenancyOptions
{
    public const string SectionName = "MultiTenancy";

    public bool Enabled { get; set; }

    public MultiTenancyMode Mode { get; set; } = MultiTenancyMode.SingleTenant;

    [Required]
    public Guid DefaultTenantId { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000000001");
}

public enum MultiTenancyMode
{
    SingleTenant = 0,
    SharedDatabase = 1
}

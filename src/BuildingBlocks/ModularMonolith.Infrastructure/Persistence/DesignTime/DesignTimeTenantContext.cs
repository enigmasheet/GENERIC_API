using ModularMonolith.SharedKernel.Abstractions;

namespace ModularMonolith.Infrastructure.Persistence.DesignTime;

public sealed class DesignTimeTenantContext : ITenantContext
{
    public static readonly Guid DefaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public Guid TenantId => DefaultTenantId;

    public bool IsResolved => true;

    public bool IsMultiTenant => false;
}

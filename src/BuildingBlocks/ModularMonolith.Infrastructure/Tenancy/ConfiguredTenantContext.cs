using Microsoft.Extensions.Options;
using ModularMonolith.Infrastructure.Options;
using ModularMonolith.SharedKernel.Abstractions;

namespace ModularMonolith.Infrastructure.Tenancy;

public sealed class ConfiguredTenantContext(IOptions<MultiTenancyOptions> options) : ITenantContext
{
    public Guid TenantId => options.Value.DefaultTenantId;

    public bool IsResolved => true;

    public bool IsMultiTenant => options.Value.Enabled;
}

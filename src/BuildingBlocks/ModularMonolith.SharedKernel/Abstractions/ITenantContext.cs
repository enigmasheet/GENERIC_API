namespace ModularMonolith.SharedKernel.Abstractions;

public interface ITenantContext
{
    Guid TenantId { get; }

    bool IsResolved { get; }

    bool IsMultiTenant { get; }
}

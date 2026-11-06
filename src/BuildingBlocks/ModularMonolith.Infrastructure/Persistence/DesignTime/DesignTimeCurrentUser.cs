using ModularMonolith.SharedKernel.Abstractions;

namespace ModularMonolith.Infrastructure.Persistence.DesignTime;

public sealed class DesignTimeCurrentUser : ICurrentUser
{
    public bool IsAuthenticated => false;

    public Guid? UserId => null;

    public string? Email => null;

    public Guid? TenantId => null;

    public Guid? SessionId => null;

    public string? IpAddress => null;

    public string? UserAgent => null;

    public IReadOnlyCollection<string> Permissions => [];
}

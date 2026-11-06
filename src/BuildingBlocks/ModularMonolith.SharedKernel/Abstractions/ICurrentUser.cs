namespace ModularMonolith.SharedKernel.Abstractions;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    Guid? UserId { get; }

    string? Email { get; }

    Guid? TenantId { get; }

    Guid? SessionId { get; }

    string? IpAddress { get; }

    string? UserAgent { get; }

    IReadOnlyCollection<string> Permissions { get; }
}

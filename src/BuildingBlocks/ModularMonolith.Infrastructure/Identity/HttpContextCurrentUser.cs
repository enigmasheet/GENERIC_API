using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ModularMonolith.SharedKernel.Abstractions;

namespace ModularMonolith.Infrastructure.Identity;

public sealed class HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public Guid? UserId => GetGuidClaim(PlatformClaims.Subject);

    public string? Email => Principal?.FindFirst(PlatformClaims.Email)?.Value;

    public Guid? TenantId => GetGuidClaim(PlatformClaims.TenantId);

    public Guid? SessionId => GetGuidClaim(PlatformClaims.SessionId);

    public string? IpAddress => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent => httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();

    public IReadOnlyCollection<string> Permissions => Principal?
        .FindAll(PlatformClaims.Permission)
        .Select(claim => claim.Value)
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .Distinct(StringComparer.Ordinal)
        .ToArray() ?? [];

    private Guid? GetGuidClaim(string claimType)
    {
        var value = Principal?.FindFirst(claimType)?.Value;

        return Guid.TryParse(value, out var id) ? id : null;
    }
}

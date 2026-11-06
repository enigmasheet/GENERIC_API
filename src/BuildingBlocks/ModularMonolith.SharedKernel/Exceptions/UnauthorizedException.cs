namespace ModularMonolith.SharedKernel.Exceptions;

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Authentication is required.", string code = "auth.unauthorized")
        : base(message, PlatformStatusCodes.Unauthorized, code)
    {
    }
}

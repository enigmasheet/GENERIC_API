namespace ModularMonolith.SharedKernel.Exceptions;

public sealed class UnauthorizedException(string message = "Authentication is required.", string code = "auth.unauthorized") : AppException(message, PlatformStatusCodes.Unauthorized, code)
{
}

namespace ModularMonolith.SharedKernel.Exceptions;

public sealed class ForbiddenException(string message = "You do not have permission to perform this action.", string code = "auth.forbidden") : AppException(message, PlatformStatusCodes.Forbidden, code)
{
}

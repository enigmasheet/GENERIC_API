namespace ModularMonolith.SharedKernel.Exceptions;

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message = "You do not have permission to perform this action.", string code = "auth.forbidden")
        : base(message, PlatformStatusCodes.Forbidden, code)
    {
    }
}

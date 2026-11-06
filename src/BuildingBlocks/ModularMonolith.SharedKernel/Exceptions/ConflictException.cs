namespace ModularMonolith.SharedKernel.Exceptions;

public sealed class ConflictException : AppException
{
    public ConflictException(string message, string code = "resource.conflict")
        : base(message, PlatformStatusCodes.Conflict, code)
    {
    }
}

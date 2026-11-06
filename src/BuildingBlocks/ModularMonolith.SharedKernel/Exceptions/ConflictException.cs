namespace ModularMonolith.SharedKernel.Exceptions;

public sealed class ConflictException(string message, string code = "resource.conflict") : AppException(message, PlatformStatusCodes.Conflict, code)
{
}

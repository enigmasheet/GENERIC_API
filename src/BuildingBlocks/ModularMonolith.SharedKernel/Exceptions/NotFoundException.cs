namespace ModularMonolith.SharedKernel.Exceptions;

public sealed class NotFoundException(string message, string code = "resource.not_found") : AppException(message, PlatformStatusCodes.NotFound, code)
{
    public static NotFoundException For(string resource, object key)
    {
        return new NotFoundException($"{resource} with identifier '{key}' was not found.");
    }
}

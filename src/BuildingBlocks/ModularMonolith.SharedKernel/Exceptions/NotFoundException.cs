namespace ModularMonolith.SharedKernel.Exceptions;

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message, string code = "resource.not_found")
        : base(message, PlatformStatusCodes.NotFound, code)
    {
    }

    public static NotFoundException For(string resource, object key)
    {
        return new NotFoundException($"{resource} with identifier '{key}' was not found.");
    }
}

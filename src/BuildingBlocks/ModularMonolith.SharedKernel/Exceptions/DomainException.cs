namespace ModularMonolith.SharedKernel.Exceptions;

public sealed class DomainException : AppException
{
    public DomainException(string message, string code = "domain.rule_violation")
        : base(message, PlatformStatusCodes.UnprocessableEntity, code)
    {
    }
}

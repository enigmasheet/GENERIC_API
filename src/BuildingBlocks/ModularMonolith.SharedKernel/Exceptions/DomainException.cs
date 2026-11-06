namespace ModularMonolith.SharedKernel.Exceptions;

public sealed class DomainException(string message, string code = "domain.rule_violation") : AppException(message, PlatformStatusCodes.UnprocessableEntity, code)
{
}

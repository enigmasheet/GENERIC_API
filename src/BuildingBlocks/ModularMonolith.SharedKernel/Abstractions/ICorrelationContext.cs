namespace ModularMonolith.SharedKernel.Abstractions;

public interface ICorrelationContext
{
    string CorrelationId { get; }

    string? TraceId { get; }
}

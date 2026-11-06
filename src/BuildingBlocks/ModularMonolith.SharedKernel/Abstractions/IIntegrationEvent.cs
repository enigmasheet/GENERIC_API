namespace ModularMonolith.SharedKernel.Abstractions;

public interface IIntegrationEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredAt { get; }

    string? CorrelationId { get; }

    Guid? TenantId { get; }
}

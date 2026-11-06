namespace ModularMonolith.SharedKernel.Abstractions;

public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();

    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;

    public string? CorrelationId { get; init; }

    public Guid? TenantId { get; init; }
}

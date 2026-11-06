using System.Text.Json;
using ModularMonolith.SharedKernel.Abstractions;

namespace ModularMonolith.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    private const int MaxErrorLength = 4000;

    private OutboxMessage()
    {
    }

    public Guid Id { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public string Payload { get; private set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public DateTimeOffset? NextAttemptAt { get; private set; }

    public int Attempts { get; private set; }

    public string? Error { get; private set; }

    public Guid? TenantId { get; private set; }

    public string? CorrelationId { get; private set; }

    public OutboxMessageStatus Status { get; private set; }

    public static OutboxMessage Create(IIntegrationEvent integrationEvent)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var payload = JsonSerializer.Serialize(
            integrationEvent,
            integrationEvent.GetType(),
            OutboxJson.Options);

        return new OutboxMessage
        {
            Id = Guid.CreateVersion7(),
            Type = integrationEvent.GetType().FullName
                ?? throw new InvalidOperationException("Integration event type must have a full name."),
            Payload = payload,
            OccurredAt = integrationEvent.OccurredAt,
            NextAttemptAt = integrationEvent.OccurredAt,
            TenantId = integrationEvent.TenantId,
            CorrelationId = integrationEvent.CorrelationId,
            Status = OutboxMessageStatus.Pending
        };
    }

    public void MarkProcessed(DateTimeOffset processedAt)
    {
        ProcessedAt = processedAt;
        NextAttemptAt = null;
        Error = null;
        Status = OutboxMessageStatus.Processed;
    }

    public void MarkFailed(DateTimeOffset failedAt, string error, int maxAttempts)
    {
        Attempts++;
        Error = Truncate(error);

        if (Attempts >= maxAttempts)
        {
            ProcessedAt = failedAt;
            NextAttemptAt = null;
            Status = OutboxMessageStatus.DeadLettered;
            return;
        }

        NextAttemptAt = failedAt.Add(CalculateBackoff(Attempts));
    }

    private static TimeSpan CalculateBackoff(int attempts)
    {
        var seconds = Math.Min(300, Math.Pow(2, Math.Min(attempts, 8)));

        return TimeSpan.FromSeconds(seconds);
    }

    private static string Truncate(string value)
    {
        return value.Length <= MaxErrorLength
            ? value
            : value[..MaxErrorLength];
    }
}

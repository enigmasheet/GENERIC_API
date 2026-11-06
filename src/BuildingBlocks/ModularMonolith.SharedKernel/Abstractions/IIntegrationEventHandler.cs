namespace ModularMonolith.SharedKernel.Abstractions;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "IntegrationEventHandler is an established architectural naming convention.")]
public interface IIntegrationEventHandler
{
    Type EventType { get; }

    Task HandleAsync(object integrationEvent, CancellationToken cancellationToken);
}
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "IntegrationEventHandler is an established architectural naming convention.")]
public abstract class IntegrationEventHandler<TEvent> : IIntegrationEventHandler
    where TEvent : IIntegrationEvent
{
    public Type EventType => typeof(TEvent);

    public abstract Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken);

    Task IIntegrationEventHandler.HandleAsync(object integrationEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        return HandleAsync((TEvent)integrationEvent, cancellationToken);
    }
}

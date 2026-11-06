namespace ModularMonolith.SharedKernel.Abstractions;

public interface IIntegrationEventHandler
{
    Type EventType { get; }

    Task HandleAsync(object integrationEvent, CancellationToken cancellationToken);
}

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

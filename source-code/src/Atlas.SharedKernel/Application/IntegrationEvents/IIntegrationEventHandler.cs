namespace Atlas.SharedKernel.Application.IntegrationEvents;

public interface IIntegrationEventHandler<in TEvent>
{
    Task HandleAsync(TEvent @event, CancellationToken ct);
}

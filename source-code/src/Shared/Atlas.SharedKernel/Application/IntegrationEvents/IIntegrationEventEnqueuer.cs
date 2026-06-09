using Atlas.SharedKernel.Domain.Events;

namespace Atlas.SharedKernel.Application.IntegrationEvents;

/// <summary>
/// Enqueues domain events as integration event outbox messages.
/// Filters only domain events that have a registered IIntegrationEventMapper,
/// builds the corresponding OutboxMessages and persists them for later dispatch.
/// </summary>
public interface IIntegrationEventEnqueuer
{
    Task EnqueueAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct);
}

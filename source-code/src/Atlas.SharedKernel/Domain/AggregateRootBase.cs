using Atlas.SharedKernel.Domain.Events;

namespace Atlas.SharedKernel.Domain;

public abstract class AggregateRootBase : IAggregateRoot
{
    private readonly List<DomainEvent> _domainEvents = new();

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents;

    protected void AddDomainEvent(DomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents()
        => _domainEvents.Clear();
}
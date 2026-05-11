using Atlas.SharedKernel.Domain.Events;

namespace Atlas.SharedKernel.Domain;

public interface IAggregateRoot
{
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
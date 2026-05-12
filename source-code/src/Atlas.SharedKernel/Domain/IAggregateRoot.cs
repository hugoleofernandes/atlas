using Atlas.SharedKernel.Domain.Events;

namespace Atlas.SharedKernel.Domain;

public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
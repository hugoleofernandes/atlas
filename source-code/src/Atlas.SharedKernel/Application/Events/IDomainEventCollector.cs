using Atlas.SharedKernel.Domain.Events;

namespace Atlas.SharedKernel.Application.Events;

public interface IDomainEventCollector
{
    void Collect(IEnumerable<DomainEvent> events);

    IReadOnlyCollection<DomainEvent> GetAll();

    void Clear();
}
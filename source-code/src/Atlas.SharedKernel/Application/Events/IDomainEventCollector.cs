using Atlas.SharedKernel.Domain.Events;

namespace Atlas.SharedKernel.Application.Events;

public interface IDomainEventCollector
{
    void Collect(IEnumerable<IDomainEvent> events);

    IReadOnlyCollection<IDomainEvent> GetAll();

    void Clear();
}
using Atlas.SharedKernel.Domain.Events;

namespace Atlas.SharedKernel.Application.Events;

public sealed class DomainEventCollector : IDomainEventCollector
{
    private readonly List<DomainEvent> _events = [];

    public void Collect(IEnumerable<DomainEvent> events)
    {
        _events.AddRange(events);
    }

    public IReadOnlyCollection<DomainEvent> GetAll()
        => _events.AsReadOnly();

    public void Clear()
        => _events.Clear();
}
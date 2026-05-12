using Atlas.SharedKernel.Domain.Events;

namespace Atlas.SharedKernel.Application.Events;

public sealed class DomainEventCollector : IDomainEventCollector
{
    private readonly List<IDomainEvent> _events = [];

    public void Collect(IEnumerable<IDomainEvent> events)
    {
        _events.AddRange(events);
    }

    public IReadOnlyCollection<IDomainEvent> GetAll()
        => _events.AsReadOnly();

    public void Clear()
        => _events.Clear();
}
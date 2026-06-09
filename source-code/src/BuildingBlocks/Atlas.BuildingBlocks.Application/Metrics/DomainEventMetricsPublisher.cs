using Atlas.SharedKernel.Application.Metrics;
using Atlas.SharedKernel.Domain.Events;

namespace Atlas.BuildingBlocks.Infrastructure.Metrics;

public sealed class DomainEventMetricsPublisher : IDomainEventMetricsPublisher
{
    // Index de metric mappers por tipo de domain event — O(1) lookup por evento
    private readonly IReadOnlyDictionary<Type, IMetricMapper> _mappers;

    public DomainEventMetricsPublisher(IEnumerable<IMetricMapper> metricMappers)
    {
        _mappers = metricMappers.ToDictionary(m => m.DomainEventType);
    }

    public void Publish(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
            ExecuteIfMapped(domainEvent);
    }

    /// <summary>
    /// Executes the metric mapper for the given domain event if one is registered for its type.
    /// Domain events without a registered metric mapper are silently ignored.
    /// </summary>
    private void ExecuteIfMapped(IDomainEvent domainEvent)
    {
        if (_mappers.TryGetValue(domainEvent.GetType(), out var mapper))
            mapper.Execute(domainEvent);
    }
}

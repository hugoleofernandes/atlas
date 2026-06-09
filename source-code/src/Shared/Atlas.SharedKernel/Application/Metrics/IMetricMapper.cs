using Atlas.SharedKernel.Domain.Events;

namespace Atlas.SharedKernel.Application.Metrics;

public interface IMetricMapper
{
    Type DomainEventType { get; }

    void Execute(IDomainEvent domainEvent);
}

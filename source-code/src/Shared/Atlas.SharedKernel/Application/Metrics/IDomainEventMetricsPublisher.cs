using Atlas.SharedKernel.Domain.Events;

namespace Atlas.SharedKernel.Application.Metrics;

public interface IDomainEventMetricsPublisher
{
    void Publish(IEnumerable<IDomainEvent> domainEvents);
}

using Atlas.BuildingBlocks.Persistence.DbContexts;
using Atlas.SharedKernel.Application.Metrics;

namespace Atlas.BuildingBlocks.Persistence.Decorators;

/// <summary>
/// Terminal step — publishes business domain event metrics to Grafana.
/// Runs after all structural steps (audit, stamp, outbox) so the metrics
/// reflect exactly what will be persisted.
/// </summary>
internal sealed class BusinessMetricsDecorator : ISavePipelineStep
{
    private readonly IDomainEventMetricsPublisher _metricsPublisher;

    public BusinessMetricsDecorator(IDomainEventMetricsPublisher metricsPublisher)
        => _metricsPublisher = metricsPublisher;

    public Task ExecuteAsync(DbContextBase db, CancellationToken ct)
    {
        var domainEvents = db.GetDomainEvents();
        _metricsPublisher.Publish(domainEvents);
        return Task.CompletedTask;
    }
}

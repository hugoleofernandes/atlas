using Atlas.BuildingBlocks.Persistence.DbContexts;
using Atlas.SharedKernel.Application.Metrics;

namespace Atlas.BuildingBlocks.Persistence.Decorators;

/// <summary>
/// Publishes business domain event metrics (Grafana dashboards) before delegating downstream.
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

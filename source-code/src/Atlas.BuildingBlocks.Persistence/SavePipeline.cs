using Atlas.BuildingBlocks.Persistence.DbContexts;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.SharedKernel.Application.Metrics;

namespace Atlas.BuildingBlocks.Persistence;

public sealed class SavePipeline : SavePipelineBase
{
    private readonly IAuditTrailService _auditTrailService;
    private readonly IEntityTenantStamper _entityTenantStamper;
    private readonly IEntityChangeStamper _entityChangeStamper;
    private readonly IIntegrationEventEnqueuer _integrationEventEnqueuer;
    private readonly IDomainEventMetricsPublisher _metricsPublisher;

    public SavePipeline(
        IAuditTrailService auditTrailService,
        IEntityTenantStamper entityTenantStamper,
        IEntityChangeStamper entityChangeStamper,
        IIntegrationEventEnqueuer integrationEventEnqueuer,
        IDomainEventMetricsPublisher metricsPublisher)
    {
        _auditTrailService = auditTrailService;
        _entityTenantStamper = entityTenantStamper;
        _entityChangeStamper = entityChangeStamper;
        _integrationEventEnqueuer = integrationEventEnqueuer;
        _metricsPublisher = metricsPublisher;
    }

    protected override async Task RunAsync(DbContextBase db, CancellationToken ct)
    {
        await _auditTrailService.RecordAsync(db, ct);
        _entityTenantStamper.Stamp(db);
        _entityChangeStamper.Stamp(db);

        var domainEvents = db.GetDomainEvents();
        _metricsPublisher.Publish(domainEvents);
        await _integrationEventEnqueuer.EnqueueAsync(domainEvents, ct);
    }
}

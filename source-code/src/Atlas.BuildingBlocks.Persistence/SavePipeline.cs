using Atlas.BuildingBlocks.Persistence.DbContexts;
using Atlas.SharedKernel.Application.IntegrationEvents;

namespace Atlas.BuildingBlocks.Persistence;

public sealed class SavePipeline : ISavePipeline
{
    private readonly IAuditTrailService _auditTrailService;
    private readonly IEntityTenantStamper _entityTenantStamper;
    private readonly IEntityChangeStamper _entityChangeStamper;
    private readonly IIntegrationEventEnqueuer _integrationEventEnqueuer;

    public SavePipeline(
        IAuditTrailService auditTrailService,
        IEntityTenantStamper entityTenantStamper,
        IEntityChangeStamper entityChangeStamper,
        IIntegrationEventEnqueuer integrationEventEnqueuer)
    {
        _auditTrailService = auditTrailService;
        _entityTenantStamper = entityTenantStamper;
        _entityChangeStamper = entityChangeStamper;
        _integrationEventEnqueuer = integrationEventEnqueuer;
    }

    public async Task ExecuteAsync(DbContextBase db, CancellationToken ct)
    {
        await _auditTrailService.RecordAsync(db, ct);
        _entityTenantStamper.Stamp(db);
        _entityChangeStamper.Stamp(db);

        var domainEvents = db.GetDomainEvents();
        await _integrationEventEnqueuer.EnqueueAsync(domainEvents, ct);
    }
}

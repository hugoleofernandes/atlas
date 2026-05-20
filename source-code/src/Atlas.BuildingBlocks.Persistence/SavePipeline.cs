using Atlas.BuildingBlocks.Persistence.DbContexts;
using Atlas.BuildingBlocks.Persistence.Decorators;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.SharedKernel.Application.Metrics;
using Microsoft.Extensions.Logging;

namespace Atlas.BuildingBlocks.Persistence;

/// <summary>
/// Composes and executes the save decorator pipeline.
/// Navigate to each decorator class to understand what it does.
/// </summary>
public sealed class SavePipeline : ISavePipeline
{
    private readonly ILogger<SavePipeline> _logger;
    private readonly IAuditTrailService _auditTrailService;
    private readonly IEntityTenantStamper _entityTenantStamper;
    private readonly IEntityChangeStamper _entityChangeStamper;
    private readonly IIntegrationEventEnqueuer _integrationEventEnqueuer;
    private readonly IDomainEventMetricsPublisher _metricsPublisher;

    public SavePipeline(
        ILogger<SavePipeline> logger,
        IAuditTrailService auditTrailService,
        IEntityTenantStamper entityTenantStamper,
        IEntityChangeStamper entityChangeStamper,
        IIntegrationEventEnqueuer integrationEventEnqueuer,
        IDomainEventMetricsPublisher metricsPublisher)
    {
        _logger                   = logger;
        _auditTrailService        = auditTrailService;
        _entityTenantStamper      = entityTenantStamper;
        _entityChangeStamper      = entityChangeStamper;
        _integrationEventEnqueuer = integrationEventEnqueuer;
        _metricsPublisher         = metricsPublisher;
    }

    public Task ExecuteAsync(DbContextBase db, CancellationToken ct)
    {
        ISavePipelineStep pipeline = new BusinessMetricsDecorator(_metricsPublisher);
        pipeline = new IntegrationEventDecorator(pipeline, _integrationEventEnqueuer);
        pipeline = new StamperDecorator(pipeline, _entityTenantStamper, _entityChangeStamper);
        pipeline = new AuditDecorator(pipeline, _auditTrailService);
        //pipeline = new LoggingDecorator(pipeline, _logger);
        //pipeline = new TelemetryDecorator(pipeline);

        return pipeline.ExecuteAsync(db, ct);
    }
}

using Atlas.BuildingBlocks.Persistence.DbContexts;
using Atlas.BuildingBlocks.Persistence.Decorators;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.SharedKernel.Application.Metrics;
using Atlas.SharedKernel.Application.OutboxMessages;
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
    private readonly IOutboxMessageBuilder _outboxMessageBuilder;
    private readonly IEnumerable<IIntegrationEventMapper> _integrationEventMappers;
    private readonly IDomainEventMetricsPublisher _metricsPublisher;

    public SavePipeline(
        ILogger<SavePipeline> logger,
        IAuditTrailService auditTrailService,
        IEntityTenantStamper entityTenantStamper,
        IEntityChangeStamper entityChangeStamper,
        IOutboxMessageBuilder outboxMessageBuilder,
        IEnumerable<IIntegrationEventMapper> integrationEventMappers,
        IDomainEventMetricsPublisher metricsPublisher)
    {
        _logger                  = logger;
        _auditTrailService       = auditTrailService;
        _entityTenantStamper     = entityTenantStamper;
        _entityChangeStamper     = entityChangeStamper;
        _outboxMessageBuilder    = outboxMessageBuilder;
        _integrationEventMappers = integrationEventMappers;
        _metricsPublisher        = metricsPublisher;
    }

    public async Task ExecuteAsync(DbContextBase db, CancellationToken ct)
    {
        // ── Business block ─────────────────────────────────────────────────────
        // Runs entirely in-memory: audit trail, entity stamping,
        // outbox enqueue and domain-event metrics.
        ISavePipelineStep businessPipeline = new BusinessMetricsDecorator(_metricsPublisher);
        businessPipeline = new IntegrationEventDecorator(businessPipeline, _outboxMessageBuilder, _integrationEventMappers);
        businessPipeline = new StamperDecorator(businessPipeline, _entityTenantStamper, _entityChangeStamper);
        businessPipeline = new AuditDecorator(businessPipeline, _auditTrailService);
        businessPipeline = new LoggingDecorator(businessPipeline, _logger, "SavePipeline.Business");
        businessPipeline = new TelemetryDecorator(businessPipeline, "SavePipeline.Business");

        await businessPipeline.ExecuteAsync(db, ct);
        // ───────────────────────────────────────────────────────────────────────

        // ── Persist block ──────────────────────────────────────────────────────
        // Flushes all tracked changes to the database.
        // EF Core emits its own SQL span automatically.
        ISavePipelineStep persistPipeline = new PersistDbDecorator();
        persistPipeline = new LoggingDecorator(persistPipeline, _logger, "SavePipeline.PersistDb");
        persistPipeline = new TelemetryDecorator(persistPipeline, "SavePipeline.PersistDb");

        await persistPipeline.ExecuteAsync(db, ct);
        // ───────────────────────────────────────────────────────────────────────
    }
}

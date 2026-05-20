using Atlas.BuildingBlocks.Persistence.DbContexts;
using Atlas.SharedKernel.Application.IntegrationEvents;

namespace Atlas.BuildingBlocks.Persistence.Decorators;

/// <summary>
/// Enqueues integration events for cross-service communication before delegating downstream.
/// </summary>
internal sealed class IntegrationEventDecorator : ISavePipelineStep
{
    private readonly ISavePipelineStep _inner;
    private readonly IIntegrationEventEnqueuer _integrationEventEnqueuer;

    public IntegrationEventDecorator(ISavePipelineStep inner, IIntegrationEventEnqueuer integrationEventEnqueuer)
    {
        _inner                    = inner;
        _integrationEventEnqueuer = integrationEventEnqueuer;
    }

    public async Task ExecuteAsync(DbContextBase db, CancellationToken ct)
    {
        var domainEvents = db.GetDomainEvents();
        await _integrationEventEnqueuer.EnqueueAsync(domainEvents, ct);
        await _inner.ExecuteAsync(db, ct);
    }
}

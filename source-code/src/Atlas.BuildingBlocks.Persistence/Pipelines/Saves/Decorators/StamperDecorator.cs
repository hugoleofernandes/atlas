using Atlas.BuildingBlocks.Persistence.DbContexts;
using Atlas.BuildingBlocks.Persistence.Entities.EntityChanges.Interfaces;
using Atlas.BuildingBlocks.Persistence.Entities.Tenants.Interfaces;
using Atlas.BuildingBlocks.Persistence.Pipelines.Saves.Interfaces;

namespace Atlas.BuildingBlocks.Persistence.Pipelines.Saves.Decorators;

/// <summary>
/// Stamps tenant and change metadata onto tracked entities before delegating downstream.
/// </summary>
internal sealed class StamperDecorator : ISavePipelineStep
{
    private readonly ISavePipelineStep _inner;
    private readonly IEntityTenantStamper _entityTenantStamper;
    private readonly IEntityChangeStamper _entityChangeStamper;

    public StamperDecorator(
        ISavePipelineStep inner,
        IEntityTenantStamper entityTenantStamper,
        IEntityChangeStamper entityChangeStamper)
    {
        _inner               = inner;
        _entityTenantStamper = entityTenantStamper;
        _entityChangeStamper = entityChangeStamper;
    }

    public async Task ExecuteAsync(DbContextBase db, CancellationToken ct)
    {
        _entityTenantStamper.Stamp(db);
        _entityChangeStamper.Stamp(db);
        await _inner.ExecuteAsync(db, ct);
    }
}

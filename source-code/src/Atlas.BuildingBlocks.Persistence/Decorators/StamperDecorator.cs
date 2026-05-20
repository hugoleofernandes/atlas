using Atlas.BuildingBlocks.Persistence.DbContexts;

namespace Atlas.BuildingBlocks.Persistence.Decorators;

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

using Atlas.BuildingBlocks.Persistence.DbContexts;
using Atlas.BuildingBlocks.Persistence.Pipelines.Saves.Interfaces;

namespace Atlas.BuildingBlocks.Persistence.Pipelines.Saves.Decorators;

/// <summary>
/// Terminal persist step — flushes all tracked changes to the database.
/// Runs after the business block has fully completed.
/// EF Core emits its own OTel span for the SQL round-trip.
/// </summary>
internal sealed class PersistDbDecorator : ISavePipelineStep
{
    public async Task ExecuteAsync(DbContextBase db, CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
        db.ClearDomainEvents();
    }
}

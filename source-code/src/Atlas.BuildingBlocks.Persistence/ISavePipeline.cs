using Atlas.BuildingBlocks.Persistence.DbContexts;

namespace Atlas.BuildingBlocks.Persistence;

public interface ISavePipeline
{
    Task ExecuteAsync(DbContextBase db, CancellationToken ct);
}

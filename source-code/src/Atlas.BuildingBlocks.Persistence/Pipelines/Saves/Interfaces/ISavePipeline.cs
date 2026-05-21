using Atlas.BuildingBlocks.Persistence.DbContexts;

namespace Atlas.BuildingBlocks.Persistence.Pipelines.Saves.Interfaces;

public interface ISavePipeline
{
    Task ExecuteAsync(DbContextBase db, CancellationToken ct);
}

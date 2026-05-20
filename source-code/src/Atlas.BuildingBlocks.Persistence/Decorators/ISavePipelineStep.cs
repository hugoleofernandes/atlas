using Atlas.BuildingBlocks.Persistence.DbContexts;

namespace Atlas.BuildingBlocks.Persistence.Decorators;

/// <summary>
/// Internal type boundary used inside the SavePipeline decorator chain.
/// Only <see cref="SavePipeline"/> implements <see cref="ISavePipeline"/> publicly —
/// everything else in this folder implements this interface instead,
/// so Ctrl+F12 on ISavePipeline resolves to a single entry point.
/// </summary>
internal interface ISavePipelineStep
{
    Task ExecuteAsync(DbContextBase db, CancellationToken ct);
}

using Atlas.BuildingBlocks.Persistence.DbContexts;
using System.Diagnostics;

namespace Atlas.BuildingBlocks.Persistence;

/// <summary>
/// Base class for all SavePipeline implementations.
/// Owns the "SavePipeline" span so subclasses stay clean.
/// </summary>
public abstract class SavePipelineBase : ISavePipeline
{
    private static readonly ActivitySource _source = new("Atlas", "1.0.0");

    public async Task ExecuteAsync(DbContextBase db, CancellationToken ct)
    {
        using var activity = _source.StartActivity("SavePipeline", ActivityKind.Internal);
        activity?.SetTag("atlas.layer", "persistence");

        await RunAsync(db, ct);

        activity?.SetStatus(ActivityStatusCode.Ok);
    }

    protected abstract Task RunAsync(DbContextBase db, CancellationToken ct);
}

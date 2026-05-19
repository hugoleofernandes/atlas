using System.Diagnostics;

namespace Atlas.BuildingBlocks.Persistence;

/// <summary>
/// Base class for all Unit of Work implementations.
/// Owns the outer "UnitOfWork SaveChanges" span.
/// Each step inside CommitAsync owns its own span:
///   - SavePipeline.ExecuteAsync → "SavePipeline" span
///   - db.SaveChangesAsync       → EF Core span (automatic)
///
/// Resulting Tempo waterfall:
///   UnitOfWork SaveChanges
///     └── SavePipeline     (audit trail + stamping + event enqueue)
///     └── 5432 INSERT      (EF Core — automatic)
/// </summary>
public abstract class UnitOfWorkBase
{
    private static readonly ActivitySource _source = new("Atlas", "1.0.0");

    protected async Task ExecuteSaveAsync(CancellationToken ct)
    {
        using var activity = _source.StartActivity("UnitOfWork SaveChanges", ActivityKind.Internal);
        activity?.SetTag("atlas.layer", "persistence");
        activity?.SetTag("atlas.uow", GetType().Name);

        await CommitAsync(ct);

        activity?.SetStatus(ActivityStatusCode.Ok);
    }

    protected abstract Task CommitAsync(CancellationToken ct);
}

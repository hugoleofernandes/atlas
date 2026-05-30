using Atlas.BuildingBlocks.Persistence.Pipelines.Saves.Interfaces;
using Atlas.Platform.Application.Abstractions;

namespace Atlas.Platform.Infrastructure.Persistence.DbContexts;

public sealed class PlatformUnitOfWork : IPlatformUnitOfWork
{
    private readonly PlatformDbContext _db;
    private readonly ISavePipeline     _savePipeline;

    public PlatformUnitOfWork(PlatformDbContext db, ISavePipeline savePipeline)
    {
        _db           = db;
        _savePipeline = savePipeline;
    }

    public Task SaveChangesAsync(CancellationToken ct)
        => _savePipeline.ExecuteAsync(_db, ct);
}

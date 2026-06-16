using Atlas.BuildingBlocks.Persistence.Pipelines.Saves.Interfaces;
using Atlas.Party.Application.Abstractions;

namespace Atlas.Party.Infrastructure.Persistence.DbContexts;

public sealed class PartyUnitOfWork : IPartyUnitOfWork
{
    private readonly PartyDbContext _db;
    private readonly ISavePipeline _savePipeline;

    public PartyUnitOfWork(PartyDbContext db, ISavePipeline savePipeline)
    {
        _db = db;
        _savePipeline = savePipeline;
    }

    public Task SaveChangesAsync(CancellationToken ct)
        => _savePipeline.ExecuteAsync(_db, ct);
}

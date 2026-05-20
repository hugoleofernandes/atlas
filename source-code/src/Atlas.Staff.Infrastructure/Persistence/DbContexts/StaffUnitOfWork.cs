using Atlas.BuildingBlocks.Persistence;
using Atlas.SharedKernel.Domain.Events;
using Atlas.Staff.Application.Abstractions;

namespace Atlas.Staff.Infrastructure.Persistence.DbContexts;

public sealed class StaffUnitOfWork : IStaffUnitOfWork
{
    private readonly StaffDbContext _db;
    private readonly ISavePipeline _savePipeline;

    public StaffUnitOfWork(StaffDbContext db, ISavePipeline savePipeline)
    {
        _db           = db;
        _savePipeline = savePipeline;
    }

    public Task SaveChangesAsync(CancellationToken ct)
        => _savePipeline.ExecuteAsync(_db, ct);

    public IEnumerable<IDomainEvent> GetDomainEvents() => _db.GetDomainEvents();
}

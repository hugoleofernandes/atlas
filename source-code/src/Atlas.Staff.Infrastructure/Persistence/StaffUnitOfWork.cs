using Atlas.SharedKernel.Application;

namespace Atlas.Staff.Infrastructure.Persistence;

public sealed class StaffUnitOfWork : IUnitOfWork
{
    private readonly StaffDbContext _db;

    public StaffUnitOfWork(StaffDbContext db)
    {
        _db = db;
    }

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);
}
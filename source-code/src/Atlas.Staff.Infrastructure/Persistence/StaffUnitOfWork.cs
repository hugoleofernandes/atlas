using Atlas.SharedKernel.Application;
using Atlas.Staff.Application.Abstractions;

namespace Atlas.Staff.Infrastructure.Persistence;

public sealed class StaffUnitOfWork : IStaffUnitOfWork
{
    private readonly StaffDbContext _db;
    private readonly IRequestContext _requestContext;

    public StaffUnitOfWork(StaffDbContext db,
        IRequestContext requestContext)
    {
        _db = db;
        _requestContext = requestContext;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        _db.ClearDomainEvents();

        await _db.SaveChangesAsync(ct);
    }

    public Task<T> GetDbContext<T>() where T : class
    {
        return Task.FromResult(_db as T ?? throw new InvalidOperationException());
    }
}
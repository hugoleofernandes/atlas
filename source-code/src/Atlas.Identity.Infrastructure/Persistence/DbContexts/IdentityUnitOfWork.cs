using Atlas.Identity.Application.Abstractions;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Infrastructure.Persistence.DbContexts;

public sealed class IdentityUnitOfWork : IIdentityUnitOfWork
{
    private readonly IdentityDbContext _db;
    private readonly IRequestContext _requestContext;

    public IdentityUnitOfWork(IdentityDbContext db, IRequestContext requestContext)
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

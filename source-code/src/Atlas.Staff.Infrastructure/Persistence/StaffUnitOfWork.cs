using Atlas.BuildingBlocks.Persistence.Outbox;
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
        var events = _db.GetDomainEvents();

        //var messages = events.Select(e =>
        //    new OutboxMessage(
        //        type: e.GetType().AssemblyQualifiedName!,
        //        payload: JsonSerializer.Serialize(e),
        //        tenantId: _requestContext.TenantId,
        //        //_db.RequestContext.TenantId,
        //        userId: _requestContext.UserId,
        //        //_db.RequestContext.UserId,
        //        correlationId: String.Empty,
        //        //_db.RequestContext.CorrelationId,
        //        module: "identity"
        //    )
        //);

        //_db.OutboxMessages.AddRange(messages);

        _db.ClearDomainEvents();

        await _db.SaveChangesAsync(ct);
    }

    public Task<T> GetDbContext<T>() where T : class
    {
        return Task.FromResult(_db as T ?? throw new InvalidOperationException());
    }
}
using Atlas.Identity.Application.Abstractions;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Infrastructure.Persistence.DbContexts;

public sealed class IdentityUnitOfWork : IIdentityUnitOfWork
{
    private readonly IdentityDbContext _db;
    private readonly IRequestContext _requestContext;
    private readonly IIntegrationEventMapper _mapper;

    public IdentityUnitOfWork(
        IdentityDbContext db,
        IRequestContext requestContext,
        IIntegrationEventMapper integrationEventMapper)
    {
        _db = db;
        _requestContext = requestContext;
        _mapper = integrationEventMapper;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        //var events = _db.GetDomainEvents();

        //var messages = events
        //    .Select(e => _mapper.Map(e, _requestContext))
        //    .Where(x => x is not null)
        //    .Cast<OutboxMessage>()
        //    .ToList();

        //_db.OutboxMessages.AddRange(messages);

        _db.ClearDomainEvents();

        await _db.SaveChangesAsync(ct);
    }

    public async Task AddOutboxMessage(OutboxMessage message) {
        _db.OutboxMessages.Add(message);
    }

    public Task<T> GetDbContext<T>() where T : class
    {
        return Task.FromResult(_db as T ?? throw new InvalidOperationException());
    }

}

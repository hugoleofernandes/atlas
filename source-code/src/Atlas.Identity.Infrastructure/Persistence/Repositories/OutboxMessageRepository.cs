using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Persistence.Repositories;

public sealed class OutboxMessageRepository : IOutboxMessageRepository
{
    private readonly IdentityDbContext _db;

    public OutboxMessageRepository(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(OutboxMessage message, CancellationToken ct)
    {
        await _db.Set<OutboxMessage>().AddAsync(message, ct);
    }

    public async Task AddRangeAsync(IEnumerable<OutboxMessage> messages, CancellationToken ct)
    {
        await _db.Set<OutboxMessage>().AddRangeAsync(messages, ct);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(CancellationToken ct)
    {
        return await _db.Set<OutboxMessage>()
            .Where(x => x.ProcessedOn == null)
            .ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _db.SaveChangesAsync(ct);
    }
}
using Atlas.SharedKernel.Application.OutboxMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;
using NpgsqlTypes;

namespace Atlas.BuildingBlocks.Persistence.OutboxMessages;

/// <summary>
/// Generic outbox repository — works with any DbContext that has OutboxMessage configured.
/// Each module registers it with its own DbContext via DI:
///   services.AddScoped&lt;IOutboxWorkerRepository, OutboxMessageRepository&lt;IdentityDbContext&gt;&gt;()
/// </summary>
public sealed class OutboxMessageRepository<TDbContext> : IOutboxMessageRepository, IOutboxWorkerRepository
    where TDbContext : DbContext
{
    private readonly TDbContext _db;

    public OutboxMessageRepository(TDbContext db)
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

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingBatchAsync(
        int batchSize, TimeSpan lockDuration, CancellationToken ct)
    {
        var batchLockId = Guid.NewGuid();
        var lockedUntil = DateTime.UtcNow.Add(lockDuration);

        var entityType  = _db.Model.FindEntityType(typeof(OutboxMessage))!;
        var schema      = entityType.GetSchema()!;
        var table       = entityType.GetTableName()!;
        var storeObject = StoreObjectIdentifier.Table(table, schema);

        // Lê os nomes reais das colunas do modelo EF — respeita qualquer convenção
        // de naming (PascalCase, snake_case, etc.) sem hardcode.
        string Col(string prop) =>
            entityType.FindProperty(prop)!.GetColumnName(storeObject);

        var locId          = Col(nameof(OutboxMessage.LockId));
        var colLockedUntil = Col(nameof(OutboxMessage.LockedUntil));
        var id             = Col(nameof(OutboxMessage.Id));
        var processedOn    = Col(nameof(OutboxMessage.ProcessedOn));
        var deadLetteredOn = Col(nameof(OutboxMessage.DeadLetteredOn));
        var occurredOn     = Col(nameof(OutboxMessage.OccurredOn));

        var sql = $"""
            UPDATE      "{schema}"."{table}"
            SET         "{locId}"          = @batchLockId,
                        "{colLockedUntil}" = @lockedUntil
            WHERE       "{id}" IN (
                SELECT  "{id}" FROM "{schema}"."{table}"
                WHERE   "{processedOn}"    IS NULL
                  AND   "{deadLetteredOn}" IS NULL
                  AND   ("{colLockedUntil}" IS NULL OR "{colLockedUntil}" < NOW())
                ORDER BY "{occurredOn}"
                LIMIT {batchSize}
                FOR UPDATE SKIP LOCKED
            )
            """;

        await _db.Database.ExecuteSqlRawAsync(
            sql,
            [
                new NpgsqlParameter("batchLockId",  NpgsqlDbType.Uuid)        { Value = batchLockId },
                new NpgsqlParameter("lockedUntil",  NpgsqlDbType.TimestampTz) { Value = lockedUntil }
            ],
            ct);

        return await _db.Set<OutboxMessage>()
            .Where(x => x.LockId == batchLockId)
            .ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _db.SaveChangesAsync(ct);
    }
}

using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.BuildingBlocks.Outbox.ListPendingMessages;

/// <summary>
/// Dapper reader that locks and fetches a batch of pending outbox messages.
/// Each module creates a concrete subclass passing the correct DbContext and schema:
///
///   public sealed class IdentityGetPendingMessagesReader(IdentityDbContext db)
///       : GetPendingMessagesReader(db, "atlas_identity") { }
///
/// The lock is implemented as an UPDATE that sets lock_id + locked_until â€” a TTL-based
/// distributed lock. No DB transaction is held open during message processing.
/// The schema string is a compile-time constant â€” safe to interpolate.
/// </summary>
public abstract class ListPendingMessagesReader : IListPendingMessagesReader
{
    private const string OutboxTableName = "outboxes";

    private readonly DbContext _db;
    private readonly string _schema;

    protected ListPendingMessagesReader(DbContext db, string schema)
    {
        _db = db;
        _schema = schema;
    }

    private string LockSql =>
        $"""
            UPDATE {_schema}.{OutboxTableName}
            SET    lock_id      = @BatchLockId,
                   locked_until = @LockedUntil
            WHERE  id IN (
                SELECT id
                FROM   {_schema}.{OutboxTableName}
                WHERE  processed_on    IS NULL
                  AND  dead_lettered_on IS NULL
                  AND  failed_at       IS NULL
                  AND  (locked_until IS NULL OR locked_until < NOW())
                ORDER BY occurred_on
                LIMIT  @BatchSize
                FOR UPDATE SKIP LOCKED
            )
            """;

    private string FetchSql =>
        $"""
            SELECT
                id              AS Id,
                name            AS Name,
                type            AS Type,
                payload         AS Payload,
                attempt_number  AS AttemptNumber,
                tenant_id       AS TenantId,
                user_id         AS UserId,
                user_email      AS UserEmail,
                correlation_id  AS CorrelationId,
                trace_parent    AS TraceParent,
                idempotency_key AS IdempotencyKey,
                locked_until    AS LockedUntil
            FROM {_schema}.{OutboxTableName}
            WHERE lock_id = @BatchLockId
            ORDER BY occurred_on
            """;

    public async Task<IReadOnlyList<ListPendingMessagesDto>> ReadAsync(
        int batchSize,
        TimeSpan lockDuration,
        CancellationToken ct
    )
    {
        var batchLockId = Guid.NewGuid();
        var lockedUntil = DateTime.UtcNow.Add(lockDuration);
        var conn = _db.Database.GetDbConnection();

        await conn.ExecuteAsync(
            new CommandDefinition(
                LockSql,
                new
                {
                    BatchLockId = batchLockId,
                    LockedUntil = lockedUntil,
                    BatchSize = batchSize,
                },
                cancellationToken: ct
            )
        );

        var results = await conn.QueryAsync<ListPendingMessagesDto>(
            new CommandDefinition(FetchSql, new { BatchLockId = batchLockId }, cancellationToken: ct)
        );

        return results.ToList().AsReadOnly();
    }
}

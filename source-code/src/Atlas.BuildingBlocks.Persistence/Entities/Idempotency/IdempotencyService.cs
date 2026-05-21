using Atlas.SharedKernel.Application.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;
using NpgsqlTypes;

namespace Atlas.BuildingBlocks.Persistence.Entities.Idempotency;

/// <summary>
/// PostgreSQL implementation of IIdempotencyService.
///
/// Uses INSERT ON CONFLICT DO NOTHING to atomically check-and-mark in a single round-trip.
/// If 0 rows are inserted the key already existed → already processed → handler should skip.
/// If 1 row is inserted the key is new → handler should proceed.
///
/// No separate SELECT needed. No race condition. No distributed lock required.
///
/// Future: swap this for RedisIdempotencyService (SET NX EX) — no handler changes needed.
/// </summary>
public sealed class IdempotencyService<TDbContext> : IIdempotencyService
    where TDbContext : DbContext
{
    private readonly TDbContext _db;
    private readonly IIdempotencyContext _context;

    public IdempotencyService(TDbContext db, IIdempotencyContext context)
    {
        _db      = db;
        _context = context;
    }

    public async Task<bool> HasAlreadyProcessedAsync(CancellationToken ct)
    {
        var entityType  = _db.Model.FindEntityType(typeof(IdempotencyEntry))!;
        var schema      = entityType.GetSchema()!;
        var table       = entityType.GetTableName()!;
        var storeObject = StoreObjectIdentifier.Table(table, schema);

        string Col(string prop) =>
            entityType.FindProperty(prop)!.GetColumnName(storeObject);

        var colIdempotencyKey = Col(nameof(IdempotencyEntry.IdempotencyKey));
        var colHandlerName    = Col(nameof(IdempotencyEntry.HandlerName));
        var colProcessedAt    = Col(nameof(IdempotencyEntry.ProcessedAt));

        var sql = $"""
            INSERT INTO "{schema}"."{table}" ("{colIdempotencyKey}", "{colHandlerName}", "{colProcessedAt}")
            VALUES (@idempotencyKey, @handlerName, @processedAt)
            ON CONFLICT ("{colIdempotencyKey}", "{colHandlerName}") DO NOTHING
            """;

        var rowsInserted = await _db.Database.ExecuteSqlRawAsync(
            sql,
            [
                new NpgsqlParameter("idempotencyKey", NpgsqlDbType.Uuid)        { Value = _context.IdempotencyKey },
                new NpgsqlParameter("handlerName",    NpgsqlDbType.Varchar)     { Value = _context.HandlerName },
                new NpgsqlParameter("processedAt",    NpgsqlDbType.TimestampTz) { Value = DateTime.UtcNow }
            ],
            ct);

        // 0 rows inserted → conflict → key already existed → already processed
        return rowsInserted == 0;
    }
}

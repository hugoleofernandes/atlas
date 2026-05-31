using Atlas.BuildingBlocks.AuditTrail.Queries;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.BuildingBlocks.Persistence.Entities.Audits;

/// <summary>
/// Base Dapper reader for listing audit entries from a module's audit table.
/// Each module creates a concrete subclass that passes the correct schema name and DbContext:
///
///   public sealed class IdentityAuditEntriesReader(IdentityDbContext db)
///       : BaseAuditEntriesReader(db, "atlas_identity") { }
///
/// The schema string is seeded/known at compile-time — safe to interpolate (not user input).
/// </summary>
public abstract class BaseAuditEntriesReader : IListAuditEntriesReader
{
    private readonly DbContext _db;
    private readonly string _schema;

    protected BaseAuditEntriesReader(DbContext db, string schema)
    {
        _db = db;
        _schema = schema;
    }

    public async Task<IReadOnlyList<AuditEntryDto>> ListAsync(
        ListAuditEntriesQuery query,
        Guid tenantId,
        CancellationToken ct
    )
    {
        var sql = $"""
            SELECT
                id               AS Id,
                entity_type_id   AS EntityTypeId,
                action           AS Action,
                entity_id        AS EntityId,
                user_id          AS UserId,
                occurred_at_utc  AS OccurredAtUtc,
                changes_json     AS ChangesJson
            FROM {_schema}.audits
            WHERE tenant_id      = @TenantId
              AND entity_type_id = @EntityTypeId
              AND (@From     IS NULL OR occurred_at_utc >= @From)
              AND (@To       IS NULL OR occurred_at_utc <= @To)
              AND (@Action   IS NULL OR action           = @Action)
              AND (@EntityId IS NULL OR entity_id        = @EntityId)
            ORDER BY occurred_at_utc DESC
            """;

        var conn = _db.Database.GetDbConnection();
        var results = await conn.QueryAsync<AuditEntryDto>(
            sql,
            new
            {
                TenantId = tenantId,
                EntityTypeId = query.EntityTypeId,
                From = query.From,
                To = query.To,
                Action = query.Action,
                EntityId = query.EntityId,
            }
        );

        return results.ToList().AsReadOnly();
    }
}

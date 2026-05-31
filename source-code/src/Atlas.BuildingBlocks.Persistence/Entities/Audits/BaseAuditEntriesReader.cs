using Atlas.BuildingBlocks.AuditTrail.Queries;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Text;

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
        var sql = new StringBuilder($"""
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
            """);

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);
        parameters.Add("EntityTypeId", query.EntityTypeId);

        if (query.From is not null)
        {
            sql.AppendLine("  AND occurred_at_utc >= @From");
            parameters.Add("From", query.From);
        }

        if (query.To is not null)
        {
            sql.AppendLine("  AND occurred_at_utc <= @To");
            parameters.Add("To", query.To);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            sql.AppendLine("  AND action = @Action");
            parameters.Add("Action", query.Action);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityId))
        {
            sql.AppendLine("  AND entity_id = @EntityId");
            parameters.Add("EntityId", query.EntityId);
        }

        sql.AppendLine("ORDER BY occurred_at_utc DESC");

        var conn = _db.Database.GetDbConnection();
        var results = await conn.QueryAsync<AuditEntryDto>(
            new CommandDefinition(sql.ToString(), parameters, cancellationToken: ct)
        );

        return results.ToList().AsReadOnly();
    }
}

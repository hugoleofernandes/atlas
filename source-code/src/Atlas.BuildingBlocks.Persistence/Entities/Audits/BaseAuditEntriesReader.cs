using Atlas.BuildingBlocks.Audit.Queries;
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
/// The schema string is seeded/known at compile-time â€” safe to interpolate (not user input).
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
                a.id              AS Id,
                a.entity_type_id  AS EntityTypeId,
                a.action          AS Action,
                a.entity_id       AS EntityId,
                a.user_id         AS UserId,
                a.user_email      AS UserEmail,
                a.occurred_at_utc AS OccurredAtUtc,
                a.changes_json    AS ChangesJson
            FROM {_schema}.audits a
            WHERE a.tenant_id      = @TenantId
              AND a.entity_type_id = @EntityTypeId

            """);

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);
        parameters.Add("EntityTypeId", query.EntityTypeId);

        if (query.From is not null)
        {
            sql.AppendLine("  AND a.occurred_at_utc >= @From");
            parameters.Add("From", query.From);
        }

        if (query.To is not null)
        {
            sql.AppendLine("  AND a.occurred_at_utc <= @To");
            parameters.Add("To", query.To);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            sql.AppendLine("  AND a.action = @Action");
            parameters.Add("Action", query.Action);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityId))
        {
            sql.AppendLine("  AND a.entity_id = @EntityId");
            parameters.Add("EntityId", query.EntityId);
        }

        sql.AppendLine("ORDER BY a.occurred_at_utc DESC");

        var conn = _db.Database.GetDbConnection();
        var results = await conn.QueryAsync<AuditEntryDto>(
            new CommandDefinition(sql.ToString(), parameters, cancellationToken: ct)
        );

        return results.ToList().AsReadOnly();
    }
}

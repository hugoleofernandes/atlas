using Atlas.Platform.Application.Queries.Audit.ListEntries;
using Atlas.Platform.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Platform.Infrastructure.Readers.Audit.ListEntries;

public sealed class ListAuditEntriesReader(PlatformDbContext db) : IListAuditEntriesReader
{
    private const string EntityTypeSql = """
        SELECT name AS Name, schema AS Schema
        FROM atlas_platform.entity_types
        WHERE id = @EntityTypeId AND is_active = true
        """;

    public async Task<IReadOnlyList<AuditEntryDto>> ListAsync(
        ListAuditEntriesQuery query,
        Guid                  tenantId,
        CancellationToken     ct)
    {
        var conn = db.Database.GetDbConnection();

        var entityType = await conn.QuerySingleOrDefaultAsync<EntityTypeRow>(
            EntityTypeSql,
            new { EntityTypeId = query.EntityTypeId });

        if (entityType is null)
            return [];

        // Schema comes from seeded data — safe to interpolate (not user input).
        var auditSql = $"""
            SELECT
                id              AS Id,
                entity_name     AS EntityName,
                action          AS Action,
                entity_id       AS EntityId,
                user_id         AS UserId,
                occurred_at_utc AS OccurredAtUtc,
                changes_json    AS ChangesJson
            FROM {entityType.Schema}.audits
            WHERE tenant_id   = @TenantId
              AND entity_name = @EntityName
              AND (@From     IS NULL OR occurred_at_utc >= @From)
              AND (@To       IS NULL OR occurred_at_utc <= @To)
              AND (@Action   IS NULL OR action           = @Action)
              AND (@EntityId IS NULL OR entity_id        = @EntityId)
            ORDER BY occurred_at_utc DESC
            """;

        var results = await conn.QueryAsync<AuditEntryDto>(auditSql, new
        {
            TenantId   = tenantId,
            EntityName = entityType.Name,
            From       = query.From,
            To         = query.To,
            Action     = query.Action,
            EntityId   = query.EntityId,
        });

        return results.ToList().AsReadOnly();
    }

    private sealed record EntityTypeRow(string Name, string Schema);
}

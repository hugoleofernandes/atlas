using Atlas.Platform.Application.Queries.Tenants.GetTenantsByIds;
using Atlas.Platform.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Platform.Infrastructure.Readers.Tenants.GetTenantsByIds;

public sealed class GetTenantsByIdsReader(PlatformDbContext db) : IGetTenantsByIdsReader
{
    private const string Sql = """
        SELECT
            t.id        AS TenantId,
            t.name      AS TenantName,
            t.is_active AS IsActive
        FROM atlas_platform.tenants t
        WHERE t.id = ANY(@Ids)
        ORDER BY t.name
        """;

    public async Task<IReadOnlyList<TenantLookupDto>> ReadAsync(IReadOnlyCollection<Guid> tenantIds, CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        var rows = await conn.QueryAsync<TenantLookupDto>(
            new CommandDefinition(Sql, new { Ids = tenantIds.ToArray() }, cancellationToken: ct));
        return rows.ToList().AsReadOnly();
    }
}

using Atlas.Platform.Application.Queries.Tenants.GetTenantByName;
using Atlas.Platform.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Platform.Infrastructure.Readers.Tenants.GetTenantByName;

public sealed class GetTenantByNameReader(PlatformDbContext db) : IGetTenantByNameReader
{
    private const string Sql = """
        SELECT
            t.id        AS TenantId,
            t.name      AS TenantName,
            t.is_active AS IsActive
        FROM atlas_platform.tenants t
        WHERE t.name = @Name
          AND t.is_active = true
        LIMIT 1
        """;

    public async Task<TenantInfoDto?> ReadAsync(string tenantName, CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        return await conn.QueryFirstOrDefaultAsync<TenantInfoDto>(Sql, new { Name = tenantName });
    }
}

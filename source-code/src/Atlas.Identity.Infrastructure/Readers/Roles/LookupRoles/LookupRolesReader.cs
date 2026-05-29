using Atlas.Identity.Application.Queries.Roles.LookupRoles;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Readers.Roles.LookupRoles;

public sealed class LookupRolesReader(IdentityDbContext db) : ILookupRolesReader
{
    private const string Sql = """
        SELECT id AS RoleId, name AS Name
        FROM atlas_identity.roles
        WHERE tenant_id = @TenantId
          AND is_active = true
        ORDER BY is_system DESC, name ASC
        """;

    public async Task<IReadOnlyList<RoleLookupDto>> LookupAsync(Guid tenantId, CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();

        var results = await conn.QueryAsync<RoleLookupDto>(
            Sql,
            new { TenantId = tenantId });

        return results.ToList();
    }
}

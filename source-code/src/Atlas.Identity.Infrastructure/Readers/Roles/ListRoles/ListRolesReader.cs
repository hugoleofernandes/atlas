using Atlas.Identity.Application.Queries.Roles.ListRoles;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Readers.Roles.ListRoles;

public sealed class ListRolesReader(IdentityDbContext db) : IListRolesReader
{
    private const string RolesSql = """
        SELECT id, name, is_system AS IsSystem
        FROM atlas_identity.roles
        WHERE tenant_id = @TenantId
          AND (@IncludeInactive OR is_active = true)
        ORDER BY is_system DESC, name ASC
        """;

    private const string PermissionsSql = """
        SELECT role_id AS RoleId, code AS Code
        FROM atlas_identity.role_permissions
        WHERE role_id = ANY(@RoleIds)
        """;

    public async Task<IReadOnlyList<RoleDto>> ListAsync(Guid tenantId, bool includeInactive, CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();

        var roles = (
            await conn.QueryAsync<RoleRow>(RolesSql, new { TenantId = tenantId, IncludeInactive = includeInactive })
        ).ToList();

        if (roles.Count == 0)
            return Array.Empty<RoleDto>();

        var roleIds = roles.Select(r => r.Id).ToArray();

        var permissions = (await conn.QueryAsync<PermissionRow>(PermissionsSql, new { RoleIds = roleIds })).ToLookup(
            p => p.RoleId
        );

        return roles
            .Select(r => new RoleDto(
                RoleId: r.Id,
                Name: r.Name,
                IsSystem: r.IsSystem,
                PermissionCodes: permissions[r.Id].Select(p => p.Code).ToList()
            ))
            .ToList();
    }

    private sealed record RoleRow(Guid Id, string Name, bool IsSystem);

    private sealed record PermissionRow(Guid RoleId, string Code);
}

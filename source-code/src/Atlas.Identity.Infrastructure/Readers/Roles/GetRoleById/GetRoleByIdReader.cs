using Atlas.Identity.Application.Queries.Roles.GetRoleById;
using Atlas.Identity.Application.Queries.Roles.ListRoles;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Readers.Roles.GetRoleById;

public sealed class GetRoleByIdReader(IdentityDbContext db) : IGetRoleByIdReader
{
    private const string RoleSql = """
        SELECT id, name, is_system AS IsSystem
        FROM atlas_identity.roles
        WHERE tenant_id = @TenantId
          AND id = @RoleId
        """;

    private const string PermissionsSql = """ 
        SELECT code AS Code
        FROM atlas_identity.role_permissions
        WHERE role_id = @RoleId
        """;

    public async Task<RoleDto?> GetByIdAsync(Guid tenantId, Guid roleId, CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();

        var role = await conn.QueryFirstOrDefaultAsync<RoleRow>(RoleSql, new { TenantId = tenantId, RoleId = roleId });

        if (role is null)
            return null;

        var permissions = (await conn.QueryAsync<string>(PermissionsSql, new { RoleId = roleId })).ToList();

        return new RoleDto(RoleId: role.Id, Name: role.Name, IsSystem: role.IsSystem, PermissionCodes: permissions);
    }

    private sealed record RoleRow(Guid Id, string Name, bool IsSystem);
}

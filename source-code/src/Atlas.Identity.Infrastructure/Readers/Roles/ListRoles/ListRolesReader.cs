using Atlas.Identity.Application.Queries.Roles.ListRoles;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Atlas.Identity.Infrastructure.Readers.Roles.ListRoles;

public sealed class ListRolesReader(IdentityDbContext db) : IListRolesReader
{
    private const string RolesSqlBase = """
        SELECT
            id,
            name,
            is_system        AS IsSystem,
            is_active        AS IsActive,
            created_at       AS CreatedAt,
            created_by       AS CreatedBy,
            created_by_email AS CreatedByEmail,
            updated_at       AS UpdatedAt,
            updated_by       AS UpdatedBy,
            updated_by_email AS UpdatedByEmail
        FROM atlas_identity.roles
        WHERE tenant_id = @TenantId
        """;

    private const string OrderBySql = """
        ORDER BY is_system DESC, name ASC
        """;

    private const string PermissionsSql = """
        SELECT role_id AS RoleId, code AS Code
        FROM atlas_identity.role_permissions
        WHERE role_id = ANY(@RoleIds)
        """;

    public async Task<IReadOnlyList<RoleDto>> ListAsync(Guid tenantId, bool? isActive, CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();

        var sql = new StringBuilder();
        sql.AppendLine(RolesSqlBase);
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);

        if (isActive is not null)
        {
            sql.AppendLine("  AND is_active = @IsActive");
            parameters.Add("IsActive", isActive.Value);
        }

        sql.AppendLine(OrderBySql);

        var roles = (
            await conn.QueryAsync<RoleRow>(sql.ToString(), parameters)
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
                IsActive: r.IsActive,
                CreatedAt: r.CreatedAt,
                CreatedBy: r.CreatedBy,
                CreatedByEmail: r.CreatedByEmail,
                UpdatedAt: r.UpdatedAt,
                UpdatedBy: r.UpdatedBy,
                UpdatedByEmail: r.UpdatedByEmail,
                PermissionCodes: permissions[r.Id].Select(p => p.Code).ToList()
            ))
            .ToList();
    }

    private sealed record RoleRow(
        Guid Id,
        string Name,
        bool IsSystem,
        bool IsActive,
        DateTime CreatedAt,
        Guid? CreatedBy,
        string? CreatedByEmail,
        DateTime? UpdatedAt,
        Guid? UpdatedBy,
        string? UpdatedByEmail);

    private sealed record PermissionRow(Guid RoleId, string Code);
}

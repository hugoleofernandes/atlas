using Atlas.Identity.Application.Queries.Users.ListUsers;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Readers.Users.ListUsers;

public sealed class ListUsersReader(IdentityDbContext db) : IListUsersReader
{
    private const string SqlBase = """
        SELECT
            u.id                  AS UserId,
            u.email,
            u.role_id             AS RoleId,
            r.name                AS RoleName,
            u.is_active           AS IsActive,
            u.created_at          AS CreatedAt,
            u.created_by          AS CreatedBy,
            u.created_by_email    AS CreatedByEmail,
            u.updated_at          AS UpdatedAt,
            u.updated_by          AS UpdatedBy,
            u.updated_by_email    AS UpdatedByEmail
        FROM atlas_identity.users u
        JOIN atlas_identity.roles r ON r.id = u.role_id
        WHERE u.tenant_id = @TenantId
        """;

    private const string IsActiveFilter = "AND u.is_active = @IsActive";
    private const string OrderBy        = "ORDER BY u.created_at DESC";

    public async Task<IReadOnlyList<UserDto>> ListAsync(Guid tenantId, bool? isActive, CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();

        var sql = isActive is null
            ? $"{SqlBase}\n{OrderBy}"
            : $"{SqlBase}\n{IsActiveFilter}\n{OrderBy}";

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);

        if (isActive is not null)
            parameters.Add("IsActive", isActive.Value);

        var result = await conn.QueryAsync<UserDto>(sql, parameters);
        return result.ToList();
    }
}

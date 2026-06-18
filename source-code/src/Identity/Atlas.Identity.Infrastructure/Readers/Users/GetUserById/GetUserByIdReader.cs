using Atlas.Identity.Application.Queries.Users.GetUserById;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Readers.Users.GetUserById;

public sealed class GetUserByIdReader(IdentityDbContext db) : IGetUserByIdReader
{
    private const string Sql = """
        SELECT
            u.id               AS UserId,
            u.email,
            u.role_id          AS RoleId,
            r.name             AS RoleName,
            u.is_active        AS IsActive,
            u.created_at       AS CreatedAt,
            u.created_by       AS CreatedBy,
            u.created_by_email AS CreatedByEmail,
            u.updated_at       AS UpdatedAt,
            u.updated_by       AS UpdatedBy,
            u.updated_by_email AS UpdatedByEmail
        FROM atlas_identity.users u
        JOIN atlas_identity.roles r ON r.id = u.role_id
        WHERE u.tenant_id = @TenantId
          AND u.id = @UserId
        """;

    public async Task<GetUserByIdDto?> GetByIdAsync(Guid tenantId, Guid userId, CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();

        return await conn.QueryFirstOrDefaultAsync<GetUserByIdDto>(
            Sql,
            new { TenantId = tenantId, UserId = userId });
    }
}

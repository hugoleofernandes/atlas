using Atlas.Identity.Application.Queries.Invitations.ListInvitations;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Readers.Invitations.ListInvitations;

public sealed class ListInvitationsReader(IdentityDbContext db) : IListInvitationsReader
{
    private const string IsActivePredicate = "NOT i.is_used AND i.expires_at >= @Now";

    private const string SqlBase = """
        SELECT
            i.id                AS InvitationId,
            i.email             AS Email,
            i.role_id           AS RoleId,
            r.name              AS RoleName,
            i.expires_at        AS ExpiresAt,
            i.is_used           AS IsUsed,
            (NOT i.is_used AND i.expires_at >= @Now) AS IsActive,
            i.created_at        AS CreatedAt,
            i.created_by        AS CreatedBy,
            i.created_by_email  AS CreatedByEmail,
            i.updated_at        AS UpdatedAt,
            i.updated_by        AS UpdatedBy,
            i.updated_by_email  AS UpdatedByEmail

        FROM atlas_identity.invitations i
        LEFT JOIN atlas_identity.roles r ON r.id = i.role_id

        WHERE i.tenant_id = @TenantId
        """;

    public async Task<IReadOnlyList<InvitationDto>> ListAsync(Guid tenantId, bool? isActive, CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();

        var sql = isActive switch
        {
            true  => $"{SqlBase} AND ({IsActivePredicate})",
            false => $"{SqlBase} AND NOT ({IsActivePredicate})",
            null  => SqlBase,
        } + "\nORDER BY i.created_at DESC, i.email ASC";

        var results = await conn.QueryAsync<InvitationDto>(sql, new { TenantId = tenantId, Now = DateTime.UtcNow });
        return results.ToList();
    }
}

using Atlas.Identity.Application.Queries.Invitations.ListInvitations;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Readers.Invitations.ListInvitations;

public sealed class ListInvitationsReader(IdentityDbContext db) : IListInvitationsReader
{
    // An invitation is active when it has not been used and has not yet expired.
    // This predicate is used twice: once to project the IsActive column, once to filter.
    private const string IsActivePredicate = "NOT i.is_used AND i.expires_at >= @Now";

    private const string SqlTemplate = """
        SELECT
            i.id                AS InvitationId,
            i.email             AS Email,
            i.role_id           AS RoleId,
            r.name              AS RoleName,
            i.expires_at        AS ExpiresAt,
            i.is_used           AS IsUsed,
            ({IsActivePredicate}) AS IsActive,
            i.created_at        AS CreatedAt,
            i.created_by        AS CreatedBy,
            i.created_by_email  AS CreatedByEmail,
            i.updated_at        AS UpdatedAt,
            i.updated_by        AS UpdatedBy,
            i.updated_by_email  AS UpdatedByEmail

        FROM atlas_identity.invitations i
        LEFT JOIN atlas_identity.roles r ON r.id = i.role_id

        WHERE i.tenant_id = @TenantId
          AND ({StatusPredicate})

        ORDER BY i.created_at DESC, i.email ASC
        """;

    public async Task<IReadOnlyList<InvitationDto>> ListAsync(Guid tenantId, bool isActive, CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        var statusPredicate = isActive ? IsActivePredicate : $"NOT ({IsActivePredicate})";
        var sql = SqlTemplate
            .Replace("{IsActivePredicate}", IsActivePredicate)
            .Replace("{StatusPredicate}", statusPredicate);

        var results = await conn.QueryAsync<InvitationDto>(sql, new { TenantId = tenantId, Now = DateTime.UtcNow });

        return results.ToList();
    }
}

using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.Identity.Application.Tenants.Queries.ListInvitations;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Entities.Tenants.Readers.ListInvitations;

public sealed class ListInvitationsReader(IdentityDbContext db) : IListInvitationsReader
{
    public async Task<IReadOnlyList<InvitationDto>> ListAsync(
        Guid tenantId,
        bool isActive,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var query = db.Invitations
            .AsNoTracking()
            .Where(i => i.TenantId == tenantId)
            .Where(i => isActive
                ? !i.IsUsed && i.ExpiresAt >= now
                : i.IsUsed || i.ExpiresAt < now)
            .OrderByDescending(i => i.CreatedAt)
            .ThenBy(i => EF.Property<string>(i, nameof(i.Email)));

        return await query
            .Join(
                db.Roles.AsNoTracking(),
                invitation => invitation.RoleId,
                role => role.Id,
                (invitation, role) => new InvitationDto(
                    invitation.Id,
                    EF.Property<string>(invitation, nameof(invitation.Email)),
                    invitation.RoleId,
                    role.Name,
                    invitation.ExpiresAt,
                    invitation.IsUsed,
                    !invitation.IsUsed && invitation.ExpiresAt >= now,
                    invitation.CreatedAt,
                    invitation.CreatedBy,
                    invitation.CreatedByEmail,
                    invitation.UpdatedAt,
                    invitation.UpdatedBy,
                    invitation.UpdatedByEmail))
            .ToListAsync(ct);
    }
}

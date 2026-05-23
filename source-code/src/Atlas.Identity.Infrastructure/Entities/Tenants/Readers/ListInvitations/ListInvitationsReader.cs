using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.Identity.Application.Tenants.Queries.ListInvitations;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Entities.Tenants.Readers.ListInvitations;

public sealed class ListInvitationsReader(IdentityDbContext db) : IListInvitationsReader
{
    public async Task<PagedResult<InvitationDto>> ListAsync(
        Guid tenantId,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var query = db.Invitations
            .AsNoTracking()
            .Where(i => i.TenantId == tenantId)
            .OrderByDescending(i => i.CreatedAt)
            .ThenBy(i => EF.Property<string>(i, nameof(i.Email)));

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
                    invitation.IsUsed))
            .ToListAsync(ct);

        return new PagedResult<InvitationDto>(items, page, pageSize, total);
    }
}

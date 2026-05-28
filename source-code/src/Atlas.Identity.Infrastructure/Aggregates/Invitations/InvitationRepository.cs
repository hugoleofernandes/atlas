using Atlas.Identity.Application.Aggregates.Invitations;
using Atlas.Identity.Domain.Invitations;
using Atlas.Identity.Domain.Shared;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Aggregates.Invitations;

public sealed class InvitationRepository : IInvitationRepository
{
    private readonly IdentityDbContext _db;

    public InvitationRepository(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task<Invitation?> FindByEmailAsync(Guid tenantId, Email email, CancellationToken ct)
    {
        return await _db.Invitations
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Email == email, ct);
    }

    public async Task<bool> HasActiveForEmailAsync(Guid tenantId, Email email, CancellationToken ct)
    {
        return await _db.Invitations
            .AnyAsync(i => i.TenantId == tenantId
                        && i.Email == email
                        && !i.IsUsed
                        && i.ExpiresAt > DateTime.UtcNow, ct);
    }

    public async Task<bool> HasActiveWithRoleAsync(Guid tenantId, Guid roleId, CancellationToken ct)
    {
        return await _db.Invitations
            .AnyAsync(i => i.TenantId == tenantId
                        && i.RoleId == roleId
                        && !i.IsUsed
                        && i.ExpiresAt > DateTime.UtcNow, ct);
    }

    public async Task<bool> HasAnyWithRoleAsync(Guid tenantId, Guid roleId, CancellationToken ct)
    {
        return await _db.Invitations
            .AnyAsync(i => i.TenantId == tenantId && i.RoleId == roleId, ct);
    }

    public async Task AddAsync(Invitation invitation, CancellationToken ct)
    {
        await _db.Invitations.AddAsync(invitation, ct);
    }
}

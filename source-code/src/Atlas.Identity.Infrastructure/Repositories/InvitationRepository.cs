using Atlas.Identity.Application.Repositories;
using Atlas.Identity.Domain.Invitations;
using Atlas.Identity.Domain.Shared;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Repositories;

public sealed class InvitationRepository : IInvitationRepository
{
    private readonly IdentityDbContext _db;

    public InvitationRepository(IdentityDbContext db) => _db = db;

    public Task<Invitation?> FindByEmailAsync(Guid tenantId, Email email, CancellationToken ct)
        => _db.Invitations
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Email == email, ct);

    public Task<bool> HasActiveForEmailAsync(Guid tenantId, Email email, CancellationToken ct)
        => _db.Invitations
            .AnyAsync(i => i.TenantId == tenantId
                        && i.Email == email
                        && !i.IsUsed
                        && i.ExpiresAt > DateTime.UtcNow, ct);

    public Task<bool> HasActiveWithRoleAsync(Guid tenantId, Guid roleId, CancellationToken ct)
        => _db.Invitations
            .AnyAsync(i => i.TenantId == tenantId
                        && i.RoleId == roleId
                        && !i.IsUsed
                        && i.ExpiresAt > DateTime.UtcNow, ct);

    public Task<bool> HasAnyWithRoleAsync(Guid tenantId, Guid roleId, CancellationToken ct)
        => _db.Invitations
            .AnyAsync(i => i.TenantId == tenantId && i.RoleId == roleId, ct);

    public async Task AddAsync(Invitation invitation, CancellationToken ct)
        => await _db.Invitations.AddAsync(invitation, ct);
}

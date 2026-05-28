using Atlas.Identity.Domain.Invitations;
using Atlas.Identity.Domain.Shared;
using Atlas.Identity.Domain.Tenants.Roles;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds the bootstrap invitation for the system owner (root role).
/// Idempotent — skips if any invitation already exists.
/// </summary>
internal sealed class InvitationSeeder
{
    public async Task SeedAsync(IServiceProvider services, CancellationToken ct)
    {
        var db = services.GetRequiredService<IdentityDbContext>();

        if (await db.Invitations.AnyAsync(ct))
            return;

        var tenant = await db.Tenants.FirstOrDefaultAsync(ct);
        if (tenant is null)
            return;

        var invitation = Invitation.Create(
            tenant.Id,
            Email.Create("hugoleofernandes@gmail.com"),
            SystemRoleIds.Root,
            InvitationTtl.Create(TimeSpan.FromHours(24))
        );

        db.Invitations.Add(invitation);
        await db.SaveChangesAsync(ct);
    }
}

using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Domain.Invitations;
using Atlas.Identity.Domain.Shared;
using Atlas.Identity.Domain.Tenants._Roles;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Atlas.Identity.Infrastructure.Seeders.Aggregates;

/// <summary>
/// Seeds the bootstrap invitation for the system owner (root role).
/// Idempotent — skips if any invitation already exists.
/// </summary>
internal sealed class InvitationSeeder
{
    public async Task SeedAsync(IServiceProvider services, CancellationToken ct)
    {
        var logger = services.GetRequiredService<ILogger<InvitationSeeder>>();
        var db     = services.GetRequiredService<IdentityDbContext>();
        var uow    = services.GetRequiredService<IIdentityUnitOfWork>();
        var setter = services.GetRequiredService<IRequestContextSetter>();

        var tenant = await db.Tenants.IgnoreQueryFilters().OrderBy(t => t.CreatedAt).FirstOrDefaultAsync(ct);
        if (tenant is null)
        {
            logger.LogWarning("InvitationSeeder skipped — no tenant found");
            return;
        }

        setter.Set(tenant.Id, tenant.Name, SystemIdentity.UserId, SystemIdentity.Email);

        if (await db.Invitations.AnyAsync(ct))
        {
            logger.LogInformation("InvitationSeeder skipped — data already exists");
            return;
        }

        logger.LogInformation("InvitationSeeder started");

        var invitation = Invitation.Create(
            tenant.Id,
            Email.Create("hugoleofernandes@gmail.com"),
            SystemRoleIds.Root,
            InvitationTtl.Create(TimeSpan.FromHours(24))
        );

        db.Invitations.Add(invitation);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("InvitationSeeder completed:");
        logger.LogInformation("  Tenant  : {Name}", tenant.Name);
        logger.LogInformation("  Email   : {Email}", invitation.Email.Value);
        logger.LogInformation("  Role    : root (system)");
        logger.LogInformation("  Expires : {ExpiresAt:u}", invitation.ExpiresAt);
    }
}

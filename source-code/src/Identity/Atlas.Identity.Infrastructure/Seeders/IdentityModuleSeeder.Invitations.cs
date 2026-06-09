using Atlas.Identity.Domain.Invitations;
using Atlas.Identity.Domain.Shared;
using Atlas.Identity.Domain.Tenants._Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlas.Identity.Infrastructure.Seeders;

public sealed partial class IdentityModuleSeeder
{
    private async Task SeedInvitationsAsync(CancellationToken ct)
    {
        if (await db.Invitations.AnyAsync(ct))
        {
            logger.LogInformation("InvitationSeeder skipped - data already exists");
            return;
        }

        logger.LogInformation("InvitationSeeder started");

        var tenantId = requestContext.TenantId
            ?? throw new InvalidOperationException("TenantId must be set in request context");

        var invitation = Invitation.Create(
            tenantId,
            Email.Create("hugoleofernandes@gmail.com"),
            SystemRoleIds.Root,
            InvitationTtl.Create(TimeSpan.FromHours(24))
        );

        db.Invitations.Add(invitation);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("InvitationSeeder completed:");
        logger.LogInformation("  Email   : {Email}", invitation.Email.Value);
        logger.LogInformation("  Role    : root (system)");
        logger.LogInformation("  Expires : {ExpiresAt:u}", invitation.ExpiresAt);
    }
}

using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Domain.Invitations;
using Atlas.Identity.Domain.Shared;
using Atlas.Identity.Domain.Tenants._Roles;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.SharedKernel.Application;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Atlas.Identity.Infrastructure.Seeders.Aggregates;

/// <summary>
/// Seeds the bootstrap invitation for the system owner (root role).
/// Reads atlas_platform.tenants via Dapper to avoid a cross-module project reference.
/// Idempotent - skips if any invitation already exists.
/// </summary>
internal sealed class InvitationSeeder
{
    private const string GetFirstTenantSql = """
        SELECT id AS TenantId, name AS TenantName
        FROM atlas_platform.tenants
        ORDER BY created_at
        LIMIT 1
        """;

    private sealed record TenantRow(Guid TenantId, string TenantName);

    public async Task SeedAsync(IServiceProvider services, CancellationToken ct)
    {
        var logger = services.GetRequiredService<ILogger<InvitationSeeder>>();
        var db = services.GetRequiredService<IdentityDbContext>();
        var uow = services.GetRequiredService<IIdentityUnitOfWork>();
        var setter = services.GetRequiredService<IRequestContextSetter>();

        var conn = db.Database.GetDbConnection();
        var tenant = await conn.QueryFirstOrDefaultAsync<TenantRow>(GetFirstTenantSql);

        if (tenant is null)
        {
            logger.LogWarning("InvitationSeeder skipped - no tenant found in atlas_platform.tenants");
            return;
        }

        setter.Set(tenant.TenantId, tenant.TenantName, SystemIdentity.UserId, SystemIdentity.Email);

        if (await db.Invitations.AnyAsync(ct))
        {
            logger.LogInformation("InvitationSeeder skipped - data already exists");
            return;
        }

        logger.LogInformation("InvitationSeeder started");

        var invitation = Invitation.Create(
            tenant.TenantId,
            Email.Create("hugoleofernandes@gmail.com"),
            SystemRoleIds.Root,
            InvitationTtl.Create(TimeSpan.FromHours(24))
        );

        db.Invitations.Add(invitation);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("InvitationSeeder completed:");
        logger.LogInformation("  Tenant  : {Name}", tenant.TenantName);
        logger.LogInformation("  Email   : {Email}", invitation.Email.Value);
        logger.LogInformation("  Role    : root (system)");
        logger.LogInformation("  Expires : {ExpiresAt:u}", invitation.ExpiresAt);
    }
}

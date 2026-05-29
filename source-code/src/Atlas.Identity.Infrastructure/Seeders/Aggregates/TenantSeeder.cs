using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Domain.Tenants;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Domain.Permissions;
using Atlas.Staff.Domain.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Atlas.Identity.Infrastructure.Seeders.Aggregates;

/// <summary>
/// Seeds the default Tenant and its system roles (root, admin, member).
/// Idempotent — skips if any tenant already exists.
/// </summary>
internal sealed class TenantSeeder
{
    public async Task SeedAsync(IServiceProvider services, CancellationToken ct)
    {
        var logger = services.GetRequiredService<ILogger<TenantSeeder>>();
        var db     = services.GetRequiredService<IdentityDbContext>();
        var uow    = services.GetRequiredService<IIdentityUnitOfWork>();
        var setter = services.GetRequiredService<IRequestContextSetter>();

        if (await db.Tenants.IgnoreQueryFilters().AnyAsync(ct))
        {
            logger.LogInformation("TenantSeeder skipped — data already exists");
            return;
        }

        logger.LogInformation("TenantSeeder started");

        var policy = services.GetRequiredService<IPermissionPolicy>();

        var memberPermissions = new[]
        {
            StaffPermissions.Read,
            StaffPermissions.Create,
            StaffPermissions.Update,
            StaffPermissions.Deactivate,
        };

        var tenant = new Tenant("tenant01");
        tenant.SeedDefaultRoles(policy.All, policy.AllIncludingSystem, memberPermissions);

        db.Tenants.Add(tenant);

        setter.Set(tenant.Id, tenant.Name, SystemIdentity.UserId, SystemIdentity.Email);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("TenantSeeder completed:");
        logger.LogInformation("  Tenant  : {Name} ({Id})", tenant.Name, tenant.Id);
        logger.LogInformation("  Roles   : {Roles}", string.Join(", ", tenant.Roles.Select(r => r.Name)));
    }
}

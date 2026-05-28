using Atlas.Identity.Domain.Tenants;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.SharedKernel.Domain.Permissions;
using Atlas.Staff.Domain.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds the default Tenant and its system roles (root, admin, member).
/// Idempotent — skips if any tenant already exists.
/// </summary>
internal sealed class TenantSeeder
{
    public async Task SeedAsync(IServiceProvider services, CancellationToken ct)
    {
        var db = services.GetRequiredService<IdentityDbContext>();

        if (await db.Tenants.AnyAsync(ct))
            return;

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
        await db.SaveChangesAsync(ct);
    }
}

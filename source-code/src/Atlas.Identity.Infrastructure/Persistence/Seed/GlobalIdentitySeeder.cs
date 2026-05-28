using Atlas.Identity.Domain.Entities.Tenants;
using Atlas.Identity.Domain.Entities.Tenants.Invitations;
using Atlas.Identity.Domain.Entities.Tenants.Roles;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.SharedKernel.Domain.Permissions;
using Atlas.Staff.Domain.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.Persistence.Seed;

public sealed class GlobalIdentitySeeder : ISeeder
{
    public async Task SeedAsync(IdentityDbContext db, IServiceProvider services)
    {
        if (await db.Tenants.AnyAsync())
            return;

        var policy = services.GetRequiredService<IPermissionPolicy>();

        // 🔹 Default member role permissions — curated set from all modules
        var memberPermissions = new[]
        {
            StaffPermissions.Read,
            StaffPermissions.Create,
            StaffPermissions.Update,
            StaffPermissions.Deactivate,
        };

        // 🔹 Create Tenant
        var tenant = new Tenant("tenant01");

        // 🔹 Seed default system roles (root, admin, member)
        tenant.SeedDefaultRoles(policy.All, policy.AllIncludingSystem, memberPermissions);

        db.Tenants.Add(tenant);

        // 🔹 Invite the system owner with the root role (fixed ID)
        tenant.InviteUser(
            Email.Create("hugoleofernandes@gmail.com"),
            SystemRoleIds.Root,
            InvitationTtl.Create(TimeSpan.FromHours(24))
        );

        await db.SaveChangesAsync();
    }
}

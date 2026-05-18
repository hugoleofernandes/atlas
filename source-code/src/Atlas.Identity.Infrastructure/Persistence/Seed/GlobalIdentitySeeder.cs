using Atlas.Identity.Domain.Entities.Tenants;
using Atlas.Identity.Domain.ValueObjects;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Persistence.Seed;

public sealed class GlobalIdentitySeeder : ISeeder
{
    public async Task SeedAsync(IdentityDbContext db, IServiceProvider services)
    {
        if (await db.Tenants.AnyAsync())
            return;

        // 🔹 Create Tenant
        var tenant = new Tenant("tenant01");

        // 🔹 Seed default system roles (admin, member, viewer)
        tenant.SeedDefaultRoles();

        db.Tenants.Add(tenant);

        // 🔹 Resolve the admin role to use in the invitation
        var adminRole = tenant.Roles.Single(r => r.Name == "admin");

        // 🔹 Invite email using the admin role
        tenant.InviteUser(
            Email.Create("hugoleofernandes@gmail.com"),
            adminRole.Id,
            InvitationTtl.Create(TimeSpan.FromHours(24))
        );

        await db.SaveChangesAsync();
    }
}

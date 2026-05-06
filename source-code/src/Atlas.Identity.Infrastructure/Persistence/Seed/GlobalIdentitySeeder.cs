using Atlas.Identity.Domain.Entities.Tenants;
using Atlas.Identity.Domain.ValueObjects;
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

        db.Tenants.Add(tenant);

        // 🔹 Invite email using Value Objects
        tenant.InviteUser(
            Email.Create("hugoleofernandes@gmail.com"),
            Role.Create("admin"),
            InvitationTtl.Create(TimeSpan.FromHours(24))
        );

        await db.SaveChangesAsync();
    }
}

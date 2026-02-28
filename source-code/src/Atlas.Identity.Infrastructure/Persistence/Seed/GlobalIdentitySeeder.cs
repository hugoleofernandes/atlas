using Atlas.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Persistence.Seed;

public sealed class GlobalIdentitySeeder : ISeeder
{
    public async Task SeedAsync(AtlasDbContext db, IServiceProvider services)
    {
        if (await db.Tenants.AnyAsync())
            return;

        // 🔹 Criar Tenant
        var tenant = new Tenant("tenant01");

        db.Tenants.Add(tenant);

        // 🔹 Convidar email (sem IdentityUser ainda)
        tenant.InviteUser(
            "hugoleofernandes@gmail.com",
            "Admin"
        );

        await db.SaveChangesAsync();
    }
}
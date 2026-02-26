using Atlas.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence.Seed;

public sealed class GlobalIdentitySeeder : ISeeder
{
    public async Task SeedAsync(AtlasDbContext db, IServiceProvider services)
    {
        if (!await db.Tenants.AnyAsync())
        {
            var tenant = new Tenant("tenant01");

            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();

            var user = new User();
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var tenantUser = new TenantUser(
                tenant.Id,
                user.Id,
                "hugoleofernandes@gmail.com",
                "Admin");

            db.TenantUsers.Add(tenantUser);

            await db.SaveChangesAsync();
        }
    }
}
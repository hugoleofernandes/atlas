using Atlas.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Persistence;

public static class AtlasDbSeeder
{
    public static async Task SeedAsync(AtlasDbContext db)
    {
        if (!await db.Tenants.AnyAsync())
        {
            var tenant = new Tenant("tenant01");

            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();

            // Criar usuário placeholder (sem OID ainda)
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
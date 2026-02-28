using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Atlas.Identity.Application.Common;

namespace Atlas.Identity.Infrastructure.Persistence.Seed;

public sealed class TestEntitySeeder : ISeeder
{
    public async Task SeedAsync(AtlasDbContext db, IServiceProvider services)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync();
        if (tenant is null)
            return;

        var tenantContext = services.GetRequiredService<ITenantContext>();
        tenantContext.Set(tenant.Id, tenant.Slug);

        if (!await db.TestEntities.AnyAsync())
        {
            db.TestEntities.Add(new TestEntity
            {
                Name = "Initial Test Data"
            });

            await db.SaveChangesAsync();
        }
    }
}
using Atlas.Application.Tenancy;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.EntityFrameworkCore;


namespace Atlas.Infrastructure.Persistence.Seed;

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
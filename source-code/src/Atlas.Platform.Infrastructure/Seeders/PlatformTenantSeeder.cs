using Atlas.Platform.Application.Abstractions;
using Atlas.Platform.Domain.Tenants;
using Atlas.Platform.Infrastructure.Persistence.DbContexts;
using Atlas.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Atlas.Platform.Infrastructure.Seeders;

/// <summary>
/// Seeds the default Tenant into atlas_platform.tenants.
/// Must run before IdentityModuleSeeder (Order = 0) so roles can be seeded against a known TenantId.
/// Idempotent — skips if any tenant already exists.
/// </summary>
internal sealed class PlatformTenantSeeder
{
    public async Task SeedAsync(IServiceProvider services, CancellationToken ct)
    {
        var logger = services.GetRequiredService<ILogger<PlatformTenantSeeder>>();
        var db     = services.GetRequiredService<PlatformDbContext>();
        var uow    = services.GetRequiredService<IPlatformUnitOfWork>();
        var setter = services.GetRequiredService<IRequestContextSetter>();

        if (await db.Tenants.AnyAsync(ct))
        {
            logger.LogInformation("PlatformTenantSeeder skipped — data already exists");
            return;
        }

        logger.LogInformation("PlatformTenantSeeder started");

        var tenant = new Tenant("tenant01");
        db.Tenants.Add(tenant);

        setter.Set(tenant.Id, tenant.Name, SystemIdentity.UserId, SystemIdentity.Email);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("PlatformTenantSeeder completed:");
        logger.LogInformation("  Tenant : {Name} ({Id})", tenant.Name, tenant.Id);
    }
}

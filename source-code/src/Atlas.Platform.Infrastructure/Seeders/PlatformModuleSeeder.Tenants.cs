using Atlas.Platform.Domain.Tenants;
using Atlas.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlas.Platform.Infrastructure.Seeders;

public sealed partial class PlatformModuleSeeder
{
    private async Task SeedTenantAsync(CancellationToken ct)
    {
        if (await db.Tenants.AnyAsync(ct))
        {
            logger.LogInformation("PlatformTenantSeeder skipped - data already exists");
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

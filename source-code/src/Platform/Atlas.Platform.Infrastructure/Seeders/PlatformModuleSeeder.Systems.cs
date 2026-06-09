using Atlas.Platform.Domain.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlas.Platform.Infrastructure.Seeders;

public sealed partial class PlatformModuleSeeder
{
    private async Task SeedSystemAsync(CancellationToken ct)
    {
        if (await db.Systems.AnyAsync(ct))
        {
            logger.LogInformation("PlatformSystemSeeder skipped - data already exists");
            return;
        }

        logger.LogInformation("PlatformSystemSeeder started");

        var system = AtlasSystem.Create("mlab");
        db.Systems.Add(system);
        logger.LogInformation("  Created {Name}", system.Name);

        await uow.SaveChangesAsync(ct);

        logger.LogInformation("PlatformSystemSeeder completed");
    }
}

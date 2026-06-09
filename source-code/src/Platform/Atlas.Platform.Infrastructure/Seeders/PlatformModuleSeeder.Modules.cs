using Atlas.Platform.Domain.Modules;
using Atlas.SharedKernel.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlas.Platform.Infrastructure.Seeders;

public sealed partial class PlatformModuleSeeder
{
    private async Task SeedModulesAsync(IReadOnlyList<AtlasModule> allModules, CancellationToken ct)
    {
        if (await db.Modules.AnyAsync(ct))
        {
            logger.LogInformation("PlatformModuleRegistrySeeder skipped - data already exists");
            return;
        }

        logger.LogInformation("PlatformModuleRegistrySeeder started");

        foreach (var module in allModules)
        {
            db.Modules.Add(Module.Create(module.Id, module.Name));
            logger.LogInformation("  Created {Name}", module.Name);
        }

        await uow.SaveChangesAsync(ct);

        logger.LogInformation("PlatformModuleRegistrySeeder completed - {Count} modules seeded", allModules.Count);
    }
}

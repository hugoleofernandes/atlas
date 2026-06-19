using Atlas.Platform.Domain.Modules;
using Atlas.SharedKernel.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlas.Platform.Infrastructure.Seeders;

public sealed partial class PlatformModuleSeeder
{
    private async Task SeedModulesAsync(IReadOnlyList<AtlasModule> allModules, CancellationToken ct)
    {
        logger.LogInformation("PlatformModuleRegistrySeeder started");

        var existing = await db.Modules
            .IgnoreQueryFilters()
            .ToListAsync(ct);

        var existingById = existing.ToDictionary(x => x.Id);

        foreach (var module in allModules)
        {
            if (!existingById.TryGetValue(module.Id, out var row))
            {
                db.Modules.Add(Module.Create(module.Id, module.Name));
                logger.LogInformation("  Created {Name}", module.Name);
            }
            else
            {
                row.Rename(module.Name);
                row.Activate();
            }
        }

        var declaredIds = allModules.Select(x => x.Id).ToHashSet();

        foreach (var row in existing.Where(x => !declaredIds.Contains(x.Id)))
        {
            row.Deactivate();
            logger.LogInformation("  Deactivated {Id} (removed from contracts)", row.Id);
        }

        await uow.SaveChangesAsync(ct);

        logger.LogInformation("PlatformModuleRegistrySeeder completed - {Count} modules declared", allModules.Count);
    }
}

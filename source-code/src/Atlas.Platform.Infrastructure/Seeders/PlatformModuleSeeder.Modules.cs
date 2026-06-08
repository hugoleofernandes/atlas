using Atlas.Platform.Domain.Modules;
using Atlas.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlas.Platform.Infrastructure.Seeders;

public sealed partial class PlatformModuleSeeder
{
    private async Task<ModuleSeedOutput> SeedModulesAsync(CancellationToken ct)
    {
        var discoveredModules = moduleDiscovery.Discover();

        if (await db.Modules.AnyAsync(ct))
        {
            logger.LogInformation("PlatformModuleRegistrySeeder skipped - data already exists");
            return new ModuleSeedOutput(
                await db.Modules.ToDictionaryAsync(module => module.Id, module => module.Id, ct));
        }

        logger.LogInformation("PlatformModuleRegistrySeeder started");

        foreach (var discoveredModule in discoveredModules)
            db.Modules.Add(Module.Create(discoveredModule.Id, discoveredModule.Name));

        setter.Set(Guid.NewGuid(), "platform-seed", SystemIdentity.UserId, SystemIdentity.Email);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("PlatformModuleRegistrySeeder completed - {Count} modules seeded", discoveredModules.Count);

        return new ModuleSeedOutput(
            discoveredModules.ToDictionary(module => module.Id, module => module.Id));
    }
}

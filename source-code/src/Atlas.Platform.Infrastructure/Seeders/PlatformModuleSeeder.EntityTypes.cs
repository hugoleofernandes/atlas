using Atlas.Platform.Application.Queries.EntityTypes.Lookup;
using Atlas.Platform.Domain.Modules;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlas.Platform.Infrastructure.Seeders;

public sealed partial class PlatformModuleSeeder
{
    private async Task SeedEntityTypesAsync(
        ModuleSeedOutput modules,
        IReadOnlyList<IModuleEntityTypes> allEntityTypes,
        IEntityTypeCatalogCache entityTypeCache,
        CancellationToken ct)
    {
        var entityTypes = allEntityTypes.SelectMany(m => m.Definitions).ToList();

        if (await db.EntityTypes.AnyAsync(ct))
        {
            logger.LogInformation("PlatformEntityTypeSeeder skipped - data already exists");
            return;
        }

        logger.LogInformation("PlatformEntityTypeSeeder started");

        foreach (var entityType in entityTypes)
        {
            if (!modules.ModuleIdsByCatalogId.TryGetValue(entityType.Module.Id, out var moduleId))
                continue;

            db.EntityTypes.Add(EntityType.Create(entityType.Id, moduleId, entityType.Name));
        }

        setter.Set(Guid.NewGuid(), "platform-seed", SystemIdentity.UserId, SystemIdentity.Email);
        await uow.SaveChangesAsync(ct);

        entityTypeCache.Invalidate();

        logger.LogInformation("PlatformEntityTypeSeeder completed - {Count} entity types seeded", entityTypes.Count);
    }
}

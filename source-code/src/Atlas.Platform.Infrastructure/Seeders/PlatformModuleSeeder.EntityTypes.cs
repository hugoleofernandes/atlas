using Atlas.Platform.Application.Queries.EntityTypes.Lookup;
using Atlas.Platform.Domain.Modules;
using Atlas.SharedKernel.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlas.Platform.Infrastructure.Seeders;

public sealed partial class PlatformModuleSeeder
{
    private async Task SeedEntityTypesAsync(
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
            db.EntityTypes.Add(EntityType.Create(entityType.Id, entityType.Module.Id, entityType.Name));
            logger.LogInformation("  Created {Module}.{Name}", entityType.Module.Name, entityType.Name);
        }

        await uow.SaveChangesAsync(ct);

        entityTypeCache.Invalidate();

        logger.LogInformation("PlatformEntityTypeSeeder completed - {Count} entity types seeded", entityTypes.Count);
    }
}

using Atlas.Platform.Application.Abstractions;
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
        ILookupEntityTypesCache entityTypeCache,
        CancellationToken ct)
    {
        logger.LogInformation("PlatformEntityTypeSeeder started");

        var declared = allEntityTypes
            .SelectMany(m => m.Definitions)
            .ToList();

        var existing = await db.EntityTypes
            .IgnoreQueryFilters()
            .ToListAsync(ct);

        var existingById = existing.ToDictionary(e => e.Id);

        foreach (var entityType in declared)
        {
            if (!existingById.TryGetValue(entityType.Id, out var row))
            {
                db.EntityTypes.Add(EntityType.Create(entityType.Id, entityType.Module.Id, entityType.Name));
                logger.LogInformation("  Created {Module}.{Name}", entityType.Module.Name, entityType.Name);
            }
            else
            {
                row.Activate();
            }
        }

        var declaredIds = declared.Select(e => e.Id).ToHashSet();

        foreach (var row in existing.Where(e => !declaredIds.Contains(e.Id)))
        {
            row.Deactivate();
            logger.LogInformation("  Deactivated {Id} (removed from contracts)", row.Id);
        }

        await uow.SaveChangesAsync(ct);

        entityTypeCache.Invalidate();

        logger.LogInformation("PlatformEntityTypeSeeder completed — {Total} entity types", declared.Count);
    }
}

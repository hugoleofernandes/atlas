using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Domain.Permissions;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atlas.Identity.Infrastructure.Seeders;

/// <summary>
/// Syncs the permission catalog in atlas_identity.permissions from module declarations.
/// Must run before any seeder that creates roles (roles reference permission IDs).
/// Behavior: upsert by Code, mark inactive when removed from contracts, never hard-delete.
/// </summary>
public sealed class IdentityPermissionCatalogSeeder(
    IdentityDbContext db,
    IIdentityUnitOfWork uow,
    IPermissionCatalogCache cache,
    ILogger<IdentityPermissionCatalogSeeder> logger)
{
    public async Task SeedAsync(IReadOnlyList<IModulePermissions> modulePermissions, CancellationToken ct)
    {
        logger.LogInformation("IdentityPermissionCatalogSeeder started");

        var existing = await db.Permissions
            .IgnoreQueryFilters()
            .ToListAsync(ct);

        var existingByCode = existing.ToDictionary(p => p.Code, StringComparer.Ordinal);

        // 1. Upsert system.root
        if (!existingByCode.TryGetValue(SystemPermissions.Root, out var root))
        {
            db.Permissions.Add(Permission.CreateRoot(Guid.NewGuid()));
            logger.LogInformation("  Created system.root");
        }
        else
        {
            root.Activate();
        }

        // 2. Collect all declared codes from modules
        var declaredCodes = new HashSet<string>(StringComparer.Ordinal);

        foreach (var module in modulePermissions)
        {
            foreach (var definition in module.Definitions)
            {
                declaredCodes.Add(definition.Code);

                if (!existingByCode.TryGetValue(definition.Code, out var permission))
                {
                    db.Permissions.Add(
                        Permission.Create(
                            Guid.NewGuid(),
                            definition.ModuleId,
                            definition.ModuleName,
                            definition.Code,
                            definition.Group,
                            definition.IsManager));
                    logger.LogInformation("  Created {Code}", definition.Code);
                }
                else
                {
                    permission.Sync(definition.ModuleId, definition.ModuleName, definition.Group, definition.IsManager);
                    permission.Activate();
                }
            }
        }

        // 3. Deactivate permissions no longer in any module (excluding system.root)
        foreach (var permission in existing)
        {
            if (permission.IsRoot)
                continue;

            if (!declaredCodes.Contains(permission.Code))
            {
                permission.Deactivate();
                logger.LogInformation("  Deactivated {Code} (removed from contracts)", permission.Code);
            }
        }

        // Request context is already set by AtlasBootstrapSeeder with tenant + root user
        await uow.SaveChangesAsync(ct);

        cache.Invalidate();

        logger.LogInformation("IdentityPermissionCatalogSeeder completed — {Total} permissions",
            existingByCode.Count + declaredCodes.Count(c => !existingByCode.ContainsKey(c)) + 1);
    }
}

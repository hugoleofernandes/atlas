using Atlas.Identity.Infrastructure.Seeders;
using Atlas.Platform.Application.Queries.EntityTypes.Lookup;
using Atlas.Platform.Infrastructure.Seeders;
using Atlas.SharedKernel.Application;
using Atlas.Staff.Infrastructure.Seeders;

namespace Atlas.API.Seeding;

/// <summary>
/// Runs startup seeders in an explicit, linear order so Atlas.API owns the
/// bootstrap flow and the required request context initialization.
///
/// Order:
///   1. Platform seeds (tenant, system, modules, entity types) — entity types passed explicitly
///   2. Permission catalog sync — must run before any role seeder
///   3. Identity seeds (system roles, bootstrap invitation)
///   4. Staff seeds (no-op currently)
/// </summary>
public sealed class AtlasBootstrapSeeder(
    IRequestContext requestContext,
    IRequestContextSetter requestContextSetter,
    PlatformModuleSeeder platformModuleSeeder,
    IdentityModuleSeeder identityModuleSeeder,
    IdentityPermissionCatalogSeeder catalogSeeder,
    StaffModuleSeeder staffModuleSeeder,
    IEntityTypeCatalogCache entityTypeCache,
    ILogger<AtlasBootstrapSeeder> logger
)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(requestContext.CorrelationId))
            requestContextSetter.SetCorrelationId(Guid.NewGuid().ToString());

        logger.LogInformation("Atlas bootstrap seeding started");

        var allEntityTypes = new[]
        {
            platformModuleSeeder.GetModuleEntityTypes(),
            identityModuleSeeder.GetModuleEntityTypes(),
            staffModuleSeeder.GetModuleEntityTypes(),
        };

        logger.LogInformation("Running PlatformModuleSeeder");
        await platformModuleSeeder.SeedAsync(allEntityTypes, entityTypeCache, ct);

        logger.LogInformation("Running IdentityPermissionCatalogSeeder");
        var allPermissions = new[]
        {
            platformModuleSeeder.GetModulePermissions(),
            identityModuleSeeder.GetModulePermissions(),
            staffModuleSeeder.GetModulePermissions(),
        };
        await catalogSeeder.SeedAsync(allPermissions, ct);

        logger.LogInformation("Running IdentityModuleSeeder");
        await identityModuleSeeder.SeedAsync(ct);

        logger.LogInformation("Running StaffModuleSeeder");
        await staffModuleSeeder.SeedAsync(ct);

        logger.LogInformation("Atlas bootstrap seeding completed");
    }
}

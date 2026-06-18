using Atlas.BuildingBlocks.Permissions;
using Atlas.Platform.Application.Abstractions;
using Atlas.Platform.Application.Queries.EntityTypes.Lookup;
using Atlas.Platform.Application.Queries.Geography.GetCitiesByState;
using Atlas.Platform.Application.Queries.Geography.GetStatesByCountry;
using Atlas.Platform.Contracts.EntityTypes;
using Atlas.Platform.Contracts.Permissions;
using Atlas.Platform.Infrastructure.Persistence.DbContexts;
using Atlas.SharedKernel.Modules;
using Microsoft.Extensions.Logging;

namespace Atlas.Platform.Infrastructure.Seeders;

/// <summary>
/// Seeds Platform bootstrap data in a linear module-owned flow.
/// Internal steps are split across partial files to keep the read path explicit.
/// </summary>
public sealed partial class PlatformModuleSeeder(
    ILogger<PlatformModuleSeeder> logger,
    PlatformDbContext db,
    IPlatformUnitOfWork uow
)
{
    public AtlasModule GetModule() => AtlasModules.Platform;

    public IModulePermissions GetModulePermissions() => new PlatformModulePermissions();

    public IModuleEntityTypes GetModuleEntityTypes() => new PlatformModuleEntityTypes();

    public async Task SeedAsync(
        IReadOnlyList<AtlasModule> allModules,
        IReadOnlyList<IModuleEntityTypes> allEntityTypes,
        ILookupEntityTypesCache entityTypeCache,
        IGetStatesByCountryCache statesCache,
        IGetCitiesByStateCache citiesCache,
        CancellationToken ct = default)
    {
        logger.LogInformation("PlatformModuleSeeder started");

        await SeedSystemAsync(ct);
        await SeedModulesAsync(allModules, ct);
        await SeedEntityTypesAsync(allEntityTypes, entityTypeCache, ct);
        await SeedGeographyAsync(statesCache, citiesCache, ct);
    }
}

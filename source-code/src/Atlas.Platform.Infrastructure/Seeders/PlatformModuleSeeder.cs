using Atlas.BuildingBlocks.Permissions;
using Atlas.Platform.Application.Abstractions;
using Atlas.Platform.Application.Queries.EntityTypes.Lookup;
using Atlas.Platform.Contracts.EntityTypes;
using Atlas.Platform.Contracts.Permissions;
using Atlas.Platform.Infrastructure.Persistence.DbContexts;
using Atlas.Platform.Infrastructure.Seeders.Discovery;
using Atlas.SharedKernel.Application;
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
    IPlatformUnitOfWork uow,
    IRequestContextSetter setter,
    IAtlasModuleDiscovery moduleDiscovery
)
{
    public IModulePermissions GetModulePermissions() => new PlatformModulePermissions();

    public IModuleEntityTypes GetModuleEntityTypes() => new PlatformModuleEntityTypes();

    public async Task SeedAsync(
        IReadOnlyList<IModuleEntityTypes> allEntityTypes,
        IEntityTypeCatalogCache entityTypeCache,
        CancellationToken ct = default)
    {
        await SeedTenantAsync(ct);
        await SeedSystemAsync(ct);

        var modules = await SeedModulesAsync(ct);
        await SeedEntityTypesAsync(modules, allEntityTypes, entityTypeCache, ct);
    }
}

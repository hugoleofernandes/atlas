using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Repositories;
using Atlas.Identity.Contracts.EntityTypes;
using Atlas.Identity.Contracts.Permissions;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Modules;
using Microsoft.Extensions.Logging;

namespace Atlas.Identity.Infrastructure.Seeders;

/// <summary>
/// Seeds Identity bootstrap data in a linear module-owned flow.
/// Internal steps are split across partial files to keep the read path explicit.
/// Must run AFTER IdentityPermissionCatalogSeeder so permission IDs exist in the database.
/// </summary>
public sealed partial class IdentityModuleSeeder(
    ILogger<IdentityModuleSeeder> logger,
    IdentityDbContext db,
    IIdentityUnitOfWork uow,
    IRequestContext requestContext,
    IPermissionCatalogCache catalogCache,
    IRoleRepository roleRepository
)
{
    public AtlasModule GetModule() => AtlasModules.Identity;

    public IModulePermissions GetModulePermissions() => new IdentityModulePermissions();

    public IModuleEntityTypes GetModuleEntityTypes() => new IdentityModuleEntityTypes();

    public async Task SeedAsync(CancellationToken ct = default)
    {
        logger.LogInformation("IdentityModuleSeeder started");

        await SeedRolesAsync(ct);
        await SeedInvitationsAsync(ct);
    }
}

using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Contracts.EntityTypes;
using Atlas.Identity.Contracts.Permissions;
using Atlas.Identity.Infrastructure.Seeders.Aggregates;
using Atlas.SharedKernel.Modules;

namespace Atlas.Identity.Infrastructure.Seeders;

/// <summary>
/// Runs all Identity-module seeders in order:
///   1. <see cref="IdentityRoleSeeder"/> - default system roles (reads Tenant from atlas_platform via Dapper)
///   2. <see cref="InvitationSeeder"/> - bootstrap invitation for the system owner
/// Must run AFTER IdentityPermissionCatalogSeeder so permission IDs exist in the database.
/// </summary>
public sealed class IdentityModuleSeeder(IServiceProvider services)
{
    public IModulePermissions GetModulePermissions() => new IdentityModulePermissions();

    public IModuleEntityTypes GetModuleEntityTypes() => new IdentityModuleEntityTypes();

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await new IdentityRoleSeeder().SeedAsync(services, ct);
        await new InvitationSeeder().SeedAsync(services, ct);
    }
}

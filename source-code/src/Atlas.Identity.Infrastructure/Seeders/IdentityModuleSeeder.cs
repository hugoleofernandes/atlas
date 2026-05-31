using Atlas.BuildingBlocks.Application.Seeding;
using Atlas.Identity.Infrastructure.Seeders.Aggregates;

namespace Atlas.Identity.Infrastructure.Seeders;

/// <summary>
/// Runs all Identity-module seeders in order:
///   1. <see cref="IdentityRoleSeeder"/>  — default system roles (reads Tenant from atlas_platform via Dapper)
///   2. <see cref="InvitationSeeder"/> — bootstrap invitation for the system owner
/// </summary>
internal sealed class IdentityModuleSeeder : IModuleSeeder
{
    public int Order => 1;

    public async Task SeedAsync(IServiceProvider services, CancellationToken ct)
    {
        await new IdentityRoleSeeder().SeedAsync(services, ct);
        await new InvitationSeeder().SeedAsync(services, ct);
    }
}

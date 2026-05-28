using Atlas.BuildingBlocks.Application.Seeding;
using Atlas.Identity.Infrastructure.Aggregates.Invitations;
using Atlas.Identity.Infrastructure.Aggregates.Tenants;

namespace Atlas.Identity.Infrastructure;

/// <summary>
/// Runs all Identity-module seeders in order:
///   1. <see cref="TenantSeeder"/>  — default Tenant + system roles
///   2. <see cref="InvitationSeeder"/> — bootstrap invitation for the system owner
/// </summary>
internal sealed class IdentityModuleSeeder : IModuleSeeder
{
    public int Order => 1;

    public async Task SeedAsync(IServiceProvider services, CancellationToken ct)
    {
        await new TenantSeeder().SeedAsync(services, ct);
        await new InvitationSeeder().SeedAsync(services, ct);
    }
}

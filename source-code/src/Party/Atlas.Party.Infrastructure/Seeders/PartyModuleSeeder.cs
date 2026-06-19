using Atlas.BuildingBlocks.Permissions;
using Atlas.Party.Contracts.EntityTypes;
using Atlas.Party.Contracts.Permissions;
using Atlas.SharedKernel.Modules;
using Microsoft.Extensions.Logging;

namespace Atlas.Party.Infrastructure.Seeders;

/// <summary>
/// Placeholder module seeder so Atlas.API can keep startup seeding explicit and linear.
/// Party currently has no bootstrap data to seed.
/// </summary>
public sealed class PartyModuleSeeder(ILogger<PartyModuleSeeder> logger)
{
    public AtlasModule GetModule() => AtlasModules.Party;

    public IModulePermissions GetModulePermissions() => new PartyModulePermissions();

    public IModuleEntityTypes GetModuleEntityTypes() => new PartyModuleEntityTypes();

    public Task SeedAsync(CancellationToken ct = default)
    {
        logger.LogInformation("PartyModuleSeeder skipped - no seed steps registered");
        return Task.CompletedTask;
    }
}

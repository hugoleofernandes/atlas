using Atlas.BuildingBlocks.Permissions;
using Atlas.SharedKernel.Modules;
using Atlas.Staff.Contracts.EntityTypes;
using Atlas.Staff.Contracts.Permissions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace Atlas.Staff.Infrastructure.Seeders;

/// <summary>
/// Placeholder module seeder so Atlas.API can keep startup seeding explicit and linear.
/// Staff currently has no bootstrap data to seed.
/// </summary>
public sealed class StaffModuleSeeder(IServiceProvider services)
{
    public IModulePermissions GetModulePermissions() => new StaffModulePermissions();

    public IModuleEntityTypes GetModuleEntityTypes() => new StaffModuleEntityTypes();

    public Task SeedAsync(CancellationToken ct = default)
    {
        var logger = services.GetRequiredService<ILogger<StaffModuleSeeder>>();
        logger.LogInformation("StaffModuleSeeder skipped - no seed steps registered");
        return Task.CompletedTask;
    }
}

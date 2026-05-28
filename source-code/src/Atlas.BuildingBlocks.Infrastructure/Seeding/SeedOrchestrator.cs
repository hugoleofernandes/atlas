namespace Atlas.BuildingBlocks.Application.Seeding;

/// <summary>
/// Collects all <see cref="IModuleSeeder"/> registrations from DI and runs them
/// in <see cref="IModuleSeeder.Order"/> sequence.
///
/// Registration (per module in its own DI extension):
///   services.AddScoped&lt;IModuleSeeder, IdentityModuleSeeder&gt;();
///
/// Usage (Atlas.API Program.cs):
///   var orchestrator = scope.ServiceProvider.GetRequiredService&lt;SeedOrchestrator&gt;();
///   await orchestrator.RunAsync(scope.ServiceProvider);
/// </summary>
public sealed class SeedOrchestrator(IEnumerable<IModuleSeeder> seeders)
{
    public async Task RunAsync(IServiceProvider services, CancellationToken ct = default)
    {
        foreach (var seeder in seeders.OrderBy(s => s.Order))
            await seeder.SeedAsync(services, ct);
    }
}

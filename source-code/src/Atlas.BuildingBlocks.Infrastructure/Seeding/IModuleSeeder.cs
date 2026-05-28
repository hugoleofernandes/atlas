namespace Atlas.BuildingBlocks.Application.Seeding;

/// <summary>
/// Marker interface for module-level seeders.
/// Each module (Identity, Staff, ...) registers one implementation.
/// The <see cref="SeedOrchestrator"/> collects all registrations via DI
/// and runs them in <see cref="Order"/> sequence.
/// </summary>
public interface IModuleSeeder
{
    /// <summary>
    /// Execution order. Lower values run first.
    /// Modules with dependencies on other modules' data must use a higher value.
    /// </summary>
    int Order { get; }

    Task SeedAsync(IServiceProvider services, CancellationToken ct);
}

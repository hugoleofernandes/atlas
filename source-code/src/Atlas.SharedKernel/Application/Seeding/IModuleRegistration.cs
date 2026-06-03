namespace Atlas.SharedKernel.Application.Seeding;

/// <summary>
/// Describes a module's identity and entity types for the Platform registry seeder.
/// Each module's Contracts project implements this and registers it in DI via Atlas.API.
/// Atlas.API is the orchestrator that knows all modules — Platform stays decoupled.
/// </summary>
public interface IModuleRegistration
{
    Guid ModuleId { get; }
    string ModuleName { get; }
    IReadOnlyList<EntityTypeRegistration> EntityTypes { get; }
}

public sealed record EntityTypeRegistration(Guid Id, string Name, string Schema);

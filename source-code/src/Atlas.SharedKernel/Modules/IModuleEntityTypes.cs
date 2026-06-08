namespace Atlas.SharedKernel.Modules;

/// <summary>
/// Implemented by each module to advertise its own entity type definitions.
/// Used by PlatformModuleSeeder to seed the entity types catalog.
/// </summary>
public interface IModuleEntityTypes
{
    Guid ModuleId { get; }
    string ModuleName { get; }
    IReadOnlyList<AtlasEntityType> Definitions { get; }
}

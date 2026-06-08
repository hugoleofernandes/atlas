namespace Atlas.BuildingBlocks.Permissions;

/// <summary>
/// Implemented by each module to advertise its own permission codes and metadata.
/// Used by IdentityPermissionCatalogSeeder to sync permissions to the database.
/// </summary>
public interface IModulePermissions
{
    Guid ModuleId { get; }
    string ModuleName { get; }
    IReadOnlyList<PermissionDefinition> Definitions { get; }
}

namespace Atlas.BuildingBlocks.Permissions;

/// <summary>
/// The aggregated permission policy for the entire application.
/// Built at startup by PermissionPolicyService from all registered IModulePermissions.
/// </summary>
public interface IPermissionPolicy
{
    IReadOnlySet<string> All { get; }
    IReadOnlySet<string> AllIncludingSystem { get; }
    IReadOnlyList<PermissionGroup> Groups { get; }
    IReadOnlyList<ModulePermissionCatalog> Modules { get; }
    IReadOnlyDictionary<string, PermissionDefinition> DefinitionsByCode { get; }
}

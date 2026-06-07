namespace Atlas.BuildingBlocks.Permissions;

/// <summary>
/// Implemented by each module to advertise its own permission codes and metadata.
/// Registered in DI as IModulePermissions so the PermissionPolicyService can
/// aggregate them all into a single IPermissionPolicy at composition time.
/// </summary>
public interface IModulePermissions
{
    Guid ModuleId { get; }
    string ModuleName { get; }
    IReadOnlySet<string> Permissions { get; }
    IReadOnlyList<PermissionGroup> Groups { get; }
    IReadOnlyList<PermissionDefinition> Definitions { get; }
}

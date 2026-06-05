namespace Atlas.BuildingBlocks.Permissions;

/// <summary>
/// Implemented by each module to advertise its own permission codes and groups.
/// Registered in DI as IModulePermissions so the PermissionPolicyService can
/// aggregate them all into a single IPermissionPolicy at composition time.
/// </summary>
public interface IModulePermissions
{
    /// <summary>Canonical module id shared with the Platform module registry.</summary>
    Guid ModuleId { get; }

    /// <summary>Stable technical module name, e.g. identity, staff, platform.</summary>
    string ModuleName { get; }

    /// <summary>All assignable permission codes owned by this module (no system.root).</summary>
    IReadOnlySet<string> Permissions { get; }

    /// <summary>Manage → granular groups owned by this module.</summary>
    IReadOnlyList<PermissionGroup> Groups { get; }
}

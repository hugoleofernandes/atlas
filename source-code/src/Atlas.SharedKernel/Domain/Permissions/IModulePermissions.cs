namespace Atlas.SharedKernel.Domain.Permissions;

/// <summary>
/// Implemented by each module to advertise its own permission codes and groups.
/// Registered in DI as IModulePermissions so the PermissionPolicyService can
/// aggregate them all into a single IPermissionPolicy at composition time.
///
/// Convention: each module owns its permission constants (e.g. StaffPermissionCatalog)
/// and returns them here. Identity never references another module's catalog directly.
/// </summary>
public interface IModulePermissions
{
    /// <summary>All assignable permission codes owned by this module (no system.root).</summary>
    IReadOnlySet<string> Permissions { get; }

    /// <summary>Manage → granular groups owned by this module.</summary>
    IReadOnlyList<PermissionGroup> Groups { get; }
}

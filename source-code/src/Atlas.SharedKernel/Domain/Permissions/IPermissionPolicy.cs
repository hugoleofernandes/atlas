namespace Atlas.SharedKernel.Domain.Permissions;

/// <summary>
/// The aggregated permission policy for the entire application.
/// Built at startup by PermissionPolicyService from all registered IModulePermissions.
///
/// Consumed by:
/// - Command handlers: validate that requested codes are legal before calling domain methods.
/// - Domain methods: receive the valid set as a parameter (no static coupling).
/// - ListPermissionsReader: expose the full catalog to the frontend.
/// </summary>
public interface IPermissionPolicy
{
    /// <summary>All assignable codes across all modules. Roles validate against this.</summary>
    IReadOnlySet<string> All { get; }

    /// <summary>Extends All with system.root. Only used when seeding system roles.</summary>
    IReadOnlySet<string> AllIncludingSystem { get; }

    /// <summary>All manage → granular groups across all modules.</summary>
    IReadOnlyList<PermissionGroup> Groups { get; }

    /// <summary>Permission catalog grouped by owning module.</summary>
    IReadOnlyList<ModulePermissionCatalog> Modules { get; }
}

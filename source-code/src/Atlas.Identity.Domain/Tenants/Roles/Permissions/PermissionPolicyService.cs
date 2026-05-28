using Atlas.SharedKernel.Domain.Permissions;

namespace Atlas.Identity.Domain.Tenants.Roles.Permissions;

/// <summary>
/// Aggregates all registered IModulePermissions into a single IPermissionPolicy.
/// Registered as a singleton in DI — built once at startup from all module registrations.
///
/// system.root is automatically added to AllIncludingSystem.
/// All other codes come exclusively from the registered modules.
/// </summary>
public sealed class PermissionPolicyService : IPermissionPolicy
{
    public PermissionPolicyService(IEnumerable<IModulePermissions> modules)
    {
        var moduleList = modules.ToList();

        All = new HashSet<string>(moduleList.SelectMany(m => m.Permissions));

        AllIncludingSystem = new HashSet<string>(All)
        {
            SystemPermissions.Root,
        };

        Groups = moduleList
            .SelectMany(m => m.Groups)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlySet<string> All { get; }
    public IReadOnlySet<string> AllIncludingSystem { get; }
    public IReadOnlyList<PermissionGroup> Groups { get; }
}

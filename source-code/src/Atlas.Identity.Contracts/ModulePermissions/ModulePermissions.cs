using Atlas.SharedKernel.Domain.Permissions;

namespace Atlas.Identity.Contracts.Permissions;

/// <summary>
/// Canonical permission codes for Identity-owned product capabilities.
/// Permission codes are shared contracts used by authorization, role management,
/// claims, and frontend permission catalogs.
/// </summary>
public sealed partial class ModulePermissions : IModulePermissions
{
    public Guid ModuleId => Module.Id;
    public string ModuleName => Module.Name;

    public IReadOnlySet<string> Permissions { get; } = PermissionExtractor.ExtractAll(typeof(ModulePermissions));

    public IReadOnlyList<PermissionGroup> Groups { get; } =
        PermissionExtractor.ExtractGroups(typeof(ModulePermissions));
}

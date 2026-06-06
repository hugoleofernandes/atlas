using Atlas.BuildingBlocks.Permissions;
using Atlas.SharedKernel.Modules;

namespace Atlas.Identity.Domain.ModulePermissions;

/// <summary>
/// Canonical permission codes for Identity-owned product capabilities.
/// Permission codes are shared contracts used by authorization, role management,
/// claims, and frontend permission catalogs.
/// </summary>
public sealed partial class IdentityModulePermissions : IModulePermissions
{
    public Guid ModuleId => AtlasModules.Identity.Id;
    public string ModuleName => AtlasModules.Identity.Name;

    public IReadOnlySet<string> Permissions { get; } =
        PermissionExtractor.ExtractAll(typeof(IdentityModulePermissions));

    public IReadOnlyList<PermissionGroup> Groups { get; } =
        PermissionExtractor.ExtractGroups(typeof(IdentityModulePermissions));
}

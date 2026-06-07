using Atlas.BuildingBlocks.Permissions;
using Atlas.SharedKernel.Modules;

namespace Atlas.Identity.Contracts.Permissions;

/// <summary>
/// Canonical permission catalog for Identity-owned product capabilities.
/// </summary>
public sealed partial class IdentityModulePermissions : ModulePermissionsBase
{
    protected override AtlasModule Module => AtlasModules.Identity;

    private static readonly IReadOnlyList<PermissionDefinition> AllDefinitions =
    [
        Users.Read,
        Roles.Read,
        Roles.Create,
        Roles.Update,
        Roles.Delete,
        Roles.Manage,
        Invitations.Read,
        Invitations.Create,
        Invitations.Update,
        Invitations.Delete,
        Invitations.Manage,
        Audit.Read,
        Audit.Manage,
    ];

    protected override IReadOnlyList<PermissionDefinition> DefinitionsCore => AllDefinitions;
}

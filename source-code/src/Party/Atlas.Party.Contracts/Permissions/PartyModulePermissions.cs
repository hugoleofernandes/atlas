using Atlas.BuildingBlocks.Permissions;
using Atlas.SharedKernel.Modules;

namespace Atlas.Party.Contracts.Permissions;

/// <summary>
/// Canonical permission catalog for Party-owned product capabilities.
/// </summary>
public sealed partial class PartyModulePermissions : IModulePermissions
{
    public Guid ModuleId => AtlasModules.Party.Id;
    public string ModuleName => AtlasModules.Party.Name;

    private static readonly IReadOnlyList<PermissionDefinition> AllDefinitions =
    [
        Individual.Read,
        Individual.Create,
        Individual.Update,
        Individual.Deactivate,
        Individual.Manage,
        Organization.Read,
        Organization.Create,
        Organization.Update,
        Organization.Deactivate,
        Organization.Manage,
    ];

    public IReadOnlyList<PermissionDefinition> Definitions => AllDefinitions;
}

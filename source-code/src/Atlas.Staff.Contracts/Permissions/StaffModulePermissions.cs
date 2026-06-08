using Atlas.BuildingBlocks.Permissions;
using Atlas.SharedKernel.Modules;

namespace Atlas.Staff.Contracts.Permissions;

public sealed partial class StaffModulePermissions : IModulePermissions
{
    public Guid ModuleId => AtlasModules.Staff.Id;
    public string ModuleName => AtlasModules.Staff.Name;

    private static readonly IReadOnlyList<PermissionDefinition> AllDefinitions =
    [
        new(StaffMember.Read, false, AtlasModules.Staff),
        new(StaffMember.Create, false, AtlasModules.Staff),
        new(StaffMember.Update, false, AtlasModules.Staff),
        new(StaffMember.Deactivate, false, AtlasModules.Staff),
        new(StaffMember.Manage, true, AtlasModules.Staff),
        new(Audit.Read, false, AtlasModules.Staff),
        new(Audit.Manage, true, AtlasModules.Staff),
    ];

    public IReadOnlyList<PermissionDefinition> Definitions => AllDefinitions;
}

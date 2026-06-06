using Atlas.BuildingBlocks.Permissions;
using Atlas.SharedKernel.EntityTypes;

namespace Atlas.Staff.Domain.ModulePermissions;

public sealed partial class StaffModulePermissions : IModulePermissions
{
    public Guid ModuleId => StaffEntityTypes.ModuleId;
    public string ModuleName => StaffEntityTypes.ModuleName;

    public IReadOnlySet<string> Permissions { get; } = PermissionExtractor.ExtractAll(typeof(StaffModulePermissions));

    public IReadOnlyList<PermissionGroup> Groups { get; } =
        PermissionExtractor.ExtractGroups(typeof(StaffModulePermissions));
}

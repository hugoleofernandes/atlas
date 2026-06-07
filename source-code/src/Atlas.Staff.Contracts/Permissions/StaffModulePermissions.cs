using Atlas.BuildingBlocks.Permissions;
using Atlas.SharedKernel.EntityTypes;

namespace Atlas.Staff.Contracts.Permissions;

public sealed partial class StaffModulePermissions : ModulePermissionsBase
{
    protected override Atlas.SharedKernel.Modules.AtlasModule Module => StaffEntityTypes.StaffMember.Module;

    protected override IReadOnlyList<PermissionDefinition> DefinitionsCore =>
        PermissionExtractor.ExtractDefinitions(
            typeof(StaffModulePermissions),
            StaffEntityTypes.StaffMember.Module.Id,
            StaffEntityTypes.StaffMember.Module.Name);
}

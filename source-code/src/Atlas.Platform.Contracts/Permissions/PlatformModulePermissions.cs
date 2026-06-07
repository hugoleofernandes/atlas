using Atlas.BuildingBlocks.Permissions;
using Atlas.SharedKernel.EntityTypes;

namespace Atlas.Platform.Contracts.Permissions;

public sealed partial class PlatformModulePermissions : ModulePermissionsBase
{
    protected override Atlas.SharedKernel.Modules.AtlasModule Module => PlatformEntityTypes.Tenant.Module;

    protected override IReadOnlyList<PermissionDefinition> DefinitionsCore =>
        PermissionExtractor.ExtractDefinitions(
            typeof(PlatformModulePermissions),
            PlatformEntityTypes.Tenant.Module.Id,
            PlatformEntityTypes.Tenant.Module.Name);
}

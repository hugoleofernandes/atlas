using Atlas.BuildingBlocks.Permissions;
using Atlas.SharedKernel.Modules;

namespace Atlas.Platform.Contracts.Permissions;

public sealed partial class PlatformModulePermissions : IModulePermissions
{
    public Guid ModuleId => AtlasModules.Platform.Id;
    public string ModuleName => AtlasModules.Platform.Name;

    private static readonly IReadOnlyList<PermissionDefinition> AllDefinitions =
    [
        new(Audit.Read, false, AtlasModules.Platform),
        new(Audit.Manage, true, AtlasModules.Platform),
    ];

    public IReadOnlyList<PermissionDefinition> Definitions => AllDefinitions;
}

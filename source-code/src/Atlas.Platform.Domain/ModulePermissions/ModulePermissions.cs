using Atlas.BuildingBlocks.Permissions;
using Atlas.SharedKernel.Platform.Domain;

namespace Atlas.Platform.Domain.ModulePermissions;

public sealed partial class ModulePermissions : IModulePermissions
{
    public Guid ModuleId => PlatformEntityTypes.ModuleId;
    public string ModuleName => PlatformEntityTypes.ModuleName;

    public IReadOnlySet<string> Permissions { get; } = PermissionExtractor.ExtractAll(typeof(ModulePermissions));

    public IReadOnlyList<PermissionGroup> Groups { get; } =
        PermissionExtractor.ExtractGroups(typeof(ModulePermissions));
}

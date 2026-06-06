using Atlas.BuildingBlocks.Permissions;
using Atlas.SharedKernel.EntityTypes;

namespace Atlas.Platform.Domain.ModulePermissions;

public sealed partial class PlatformModulePermissions : IModulePermissions
{
    public Guid ModuleId => PlatformEntityTypes.Tenant.Module.Id;
    public string ModuleName => PlatformEntityTypes.Tenant.Module.Name;

    public IReadOnlySet<string> Permissions { get; } =
        PermissionExtractor.ExtractAll(typeof(PlatformModulePermissions));

    public IReadOnlyList<PermissionGroup> Groups { get; } =
        PermissionExtractor.ExtractGroups(typeof(PlatformModulePermissions));
}

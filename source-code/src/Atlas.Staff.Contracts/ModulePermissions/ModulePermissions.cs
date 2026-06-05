using Atlas.BuildingBlocks.Permissions;

namespace Atlas.Staff.Contracts.Permissions;

public sealed partial class ModulePermissions : IModulePermissions
{
    public Guid ModuleId => Module.Id;
    public string ModuleName => Module.Name;

    public IReadOnlySet<string> Permissions { get; } = PermissionExtractor.ExtractAll(typeof(ModulePermissions));

    public IReadOnlyList<PermissionGroup> Groups { get; } =
        PermissionExtractor.ExtractGroups(typeof(ModulePermissions));
}

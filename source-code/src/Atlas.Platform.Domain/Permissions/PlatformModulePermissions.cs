using Atlas.SharedKernel.Domain.Permissions;

namespace Atlas.Platform.Domain.Permissions;

/// <summary>
/// Single source of truth for Platform module permissions.
/// Convention: a nested class with a "Manage" field produces one PermissionGroup.
/// </summary>
public sealed class PlatformModulePermissions : IModulePermissions
{
    public static class Audit
    {
        public const string Read   = "platform.audit.read";
        public const string Manage = "platform.audit.manage";
    }

    public IReadOnlySet<string> Permissions { get; }
        = PermissionExtractor.ExtractAll(typeof(PlatformModulePermissions));

    public IReadOnlyList<PermissionGroup> Groups { get; }
        = PermissionExtractor.ExtractGroups(typeof(PlatformModulePermissions));
}

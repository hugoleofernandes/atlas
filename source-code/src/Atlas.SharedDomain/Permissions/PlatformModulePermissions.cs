using Atlas.SharedDomain.Modules;
using Atlas.SharedKernel.Domain.Permissions;

namespace Atlas.SharedDomain.Permissions;

/// <summary>
/// Canonical permission codes for Platform-owned product capabilities.
/// Permission codes are shared contracts used by authorization, role management,
/// claims, and frontend permission catalogs.
/// </summary>
public sealed class PlatformModulePermissions : IModulePermissions
{
    public Guid ModuleId => AtlasModules.Platform;
    public string ModuleName => AtlasModules.PlatformName;

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

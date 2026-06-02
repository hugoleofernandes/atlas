using Atlas.SharedDomain.Modules;
using Atlas.SharedKernel.Domain.Permissions;

namespace Atlas.SharedDomain.Permissions;

/// <summary>
/// Canonical permission codes for Staff-owned product capabilities.
/// Permission codes are shared contracts used by authorization, role management,
/// claims, and frontend permission catalogs.
/// </summary>
public sealed class StaffPermissions : IModulePermissions
{
    public Guid ModuleId => AtlasModules.Staff;
    public string ModuleName => AtlasModules.StaffName;

    public const string Read       = "staff.read";
    public const string Create     = "staff.create";
    public const string Update     = "staff.update";
    public const string Deactivate = "staff.deactivate";
    public const string Manage     = "staff.manage";

    public static class Audit
    {
        public const string Read   = "staff.audit.read";
        public const string Manage = "staff.audit.manage";
    }

    public IReadOnlySet<string> Permissions { get; }
        = PermissionExtractor.ExtractAll(typeof(StaffPermissions));

    public IReadOnlyList<PermissionGroup> Groups { get; }
        = PermissionExtractor.ExtractGroups(typeof(StaffPermissions));
}

using Atlas.SharedKernel.Domain.Permissions;

namespace Atlas.Staff.Domain.Permissions;

/// <summary>
/// Single source of truth for Staff module permissions.
/// Declares the permission codes as typed constants (used in [HasPermission] attributes)
/// and implements IModulePermissions so the codes and groups are automatically derived
/// via PermissionExtractor — no duplication required.
///
/// Convention: a class with a "Manage" field produces one PermissionGroup.
/// </summary>
public sealed class StaffPermissions : IModulePermissions
{
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

namespace Atlas.Staff.Contracts.Permissions;

public sealed partial class StaffModulePermissions
{
    public static class Audit
    {
        public const string Read = "staff.audit.read";
        public const string Manage = "staff.audit.manage";
    }
}

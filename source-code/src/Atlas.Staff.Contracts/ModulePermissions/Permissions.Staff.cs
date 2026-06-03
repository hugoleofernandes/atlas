namespace Atlas.Staff.Contracts.Permissions;

public sealed partial class ModulePermissions
{
    public static class Staff
    {
        public const string Read       = "staff.read";
        public const string Create     = "staff.create";
        public const string Update     = "staff.update";
        public const string Deactivate = "staff.deactivate";
        public const string Manage     = "staff.manage";
    }
}

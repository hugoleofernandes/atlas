namespace Atlas.Staff.Contracts.Permissions;

public sealed partial class StaffModulePermissions
{
    public static class StaffMember
    {
        public const string Read = "staff.staff-member.read";
        public const string Create = "staff.staff-member.create";
        public const string Update = "staff.staff-member.update";
        public const string Deactivate = "staff.staff-member.deactivate";
        public const string Manage = "staff.staff-member.manage";
    }
}

namespace Atlas.Staff.Domain.ModulePermissions;

public sealed partial class StaffModulePermissions
{
    public static class StaffMember
    {
        public const string Read = "staff-member.read";
        public const string Create = "staff-member.create";
        public const string Update = "staff-member.update";
        public const string Deactivate = "staff-member.deactivate";
        public const string Manage = "staff-member.manage";
    }
}

namespace Atlas.Staff.Contracts.Permissions;

public sealed partial class StaffModulePermissions
{
    public static class Outbox
    {
        public const string Read     = "staff.outbox.read";
        public const string Resubmit = "staff.outbox.resubmit";
        public const string Process  = "staff.outbox.process";
        public const string Manage   = "staff.outbox.manage";
    }
}

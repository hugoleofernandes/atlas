namespace Atlas.Identity.Contracts.Permissions;

public sealed partial class ModulePermissions
{
    public static class Audit
    {
        public const string Read = "identity.audit.read";
        public const string Manage = "identity.audit.manage";
    }
}

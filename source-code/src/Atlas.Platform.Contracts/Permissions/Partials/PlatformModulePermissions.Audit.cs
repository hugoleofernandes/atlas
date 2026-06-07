namespace Atlas.Platform.Contracts.Permissions;

public sealed partial class PlatformModulePermissions
{
    public static class Audit
    {
        public const string Read = "platform.audit.read";
        public const string Manage = "platform.audit.manage";
    }
}

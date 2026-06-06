namespace Atlas.Identity.Domain.ModulePermissions;

public sealed partial class IdentityModulePermissions
{
    public static class Roles
    {
        public const string Read = "identity.roles.read";
        public const string Create = "identity.roles.create";
        public const string Update = "identity.roles.update";
        public const string Delete = "identity.roles.delete";
        public const string Manage = "identity.roles.manage";
    }
}

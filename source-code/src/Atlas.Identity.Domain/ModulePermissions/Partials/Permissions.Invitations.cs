namespace Atlas.Identity.Domain.ModulePermissions;

public sealed partial class IdentityModulePermissions
{
    public static class Invitations
    {
        public const string Read = "identity.invitations.read";
        public const string Create = "identity.invitations.create";
        public const string Update = "identity.invitations.update";
        public const string Delete = "identity.invitations.delete";
        public const string Manage = "identity.invitations.manage";
    }
}

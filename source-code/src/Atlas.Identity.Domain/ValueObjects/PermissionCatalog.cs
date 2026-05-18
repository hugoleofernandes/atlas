namespace Atlas.Identity.Domain.ValueObjects;

/// <summary>
/// The authoritative set of permissions that exist in the system.
/// Clients configure which permissions each of their roles has —
/// but they cannot invent permissions outside this catalog.
/// </summary>
public static class PermissionCatalog
{
    public static class Staff
    {
        public const string Read       = "staff.read";
        public const string Create     = "staff.create";
        public const string Update     = "staff.update";
        public const string Deactivate = "staff.deactivate";
    }

    public static class Tenant
    {
        public const string InviteUser  = "tenant.invite_user";
        public const string ManageRoles = "tenant.manage_roles";
    }

    public static IReadOnlySet<string> All { get; } = new HashSet<string>
    {
        Staff.Read,
        Staff.Create,
        Staff.Update,
        Staff.Deactivate,
        Tenant.InviteUser,
        Tenant.ManageRoles,
    };
}

namespace Atlas.Identity.Domain.Permissions;

/// <summary>
/// The authoritative set of permissions that exist in the system.
/// Clients configure which permissions each of their roles has —
/// but they cannot invent permissions outside this catalog.
/// </summary>
public static class PermissionCatalog
{
    public static class System
    {
        // Grants unrestricted access — bypasses all permission checks.
        // Only assignable to system roles (isSystem=true).
        public const string Root = "system.root";
    }

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

    // Custom roles validate against this set — system.root is excluded on purpose.
    public static IReadOnlySet<string> All { get; } = new HashSet<string>
    {
        Staff.Read,
        Staff.Create,
        Staff.Update,
        Staff.Deactivate,
        Tenant.InviteUser,
        Tenant.ManageRoles,
    };

    // Only used when seeding system roles (isSystem=true).
    public static IReadOnlySet<string> AllIncludingSystem { get; } = new HashSet<string>(All)
    {
        System.Root,
    };
}

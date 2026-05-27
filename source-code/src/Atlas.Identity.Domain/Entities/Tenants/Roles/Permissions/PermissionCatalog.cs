namespace Atlas.Identity.Domain.Entities.Tenants.Roles.Permissions;

/// <summary>
/// Declares the relationship between a manage permission and its implied granular verbs.
/// Used by the frontend to render grouped checkboxes: selecting manage collapses its children.
/// </summary>
public sealed record PermissionGroup(string Manage, IReadOnlyList<string> Granular);

/// <summary>
/// The authoritative set of permissions that exist in the system.
/// Clients configure which permissions each of their roles has —
/// but they cannot invent permissions outside this catalog.
///
/// Naming convention: {module}.{resource}.{verb}
/// Verbs: read | create | update | delete | manage (= all verbs for that resource)
/// manage is an assignment shortcut — never used in [HasPermission] on endpoints.
/// </summary>
public static class PermissionCatalog
{
    public static class System
    {
        // Grants unrestricted access — bypasses all permission checks.
        // Only assignable to system roles (isSystem=true).
        public const string Root = "system.root";
    }

    public static class Tenant
    {
        public static class Roles
        {
            public const string Read   = "tenant.roles.read";
            public const string Create = "tenant.roles.create";
            public const string Update = "tenant.roles.update";
            public const string Delete = "tenant.roles.delete";
            public const string Manage = "tenant.roles.manage";
        }

        public static class Invitations
        {
            public const string Read   = "tenant.invitations.read";
            public const string Create = "tenant.invitations.create";
            public const string Update = "tenant.invitations.update";
            public const string Delete = "tenant.invitations.delete";
            public const string Manage = "tenant.invitations.manage";
        }
    }

    public static class Staff
    {
        public const string Read       = "staff.read";
        public const string Create     = "staff.create";
        public const string Update     = "staff.update";
        public const string Deactivate = "staff.deactivate";
        public const string Manage     = "staff.manage";
    }

    // Custom roles validate against this set — system.root is excluded on purpose.
    public static IReadOnlySet<string> All { get; } = new HashSet<string>
    {
        Tenant.Roles.Read,
        Tenant.Roles.Create,
        Tenant.Roles.Update,
        Tenant.Roles.Delete,
        Tenant.Roles.Manage,

        Tenant.Invitations.Read,
        Tenant.Invitations.Create,
        Tenant.Invitations.Update,
        Tenant.Invitations.Delete,
        Tenant.Invitations.Manage,

        Staff.Read,
        Staff.Create,
        Staff.Update,
        Staff.Deactivate,
        Staff.Manage,
    };

    // Only used when seeding system roles (isSystem=true).
    public static IReadOnlySet<string> AllIncludingSystem { get; } = new HashSet<string>(All)
    {
        System.Root,
    };

    /// <summary>
    /// Declares the manage → granular relationship for each resource.
    /// Consumed by the frontend to render grouped permission selectors:
    /// selecting manage implies all its granular children.
    /// </summary>
    public static IReadOnlyList<PermissionGroup> Groups { get; } =
    [
        new(Tenant.Roles.Manage,
        [
            Tenant.Roles.Read,
            Tenant.Roles.Create,
            Tenant.Roles.Update,
            Tenant.Roles.Delete,
        ]),

        new(Tenant.Invitations.Manage,
        [
            Tenant.Invitations.Read,
            Tenant.Invitations.Create,
            Tenant.Invitations.Update,
            Tenant.Invitations.Delete,
        ]),

        new(Staff.Manage,
        [
            Staff.Read,
            Staff.Create,
            Staff.Update,
            Staff.Deactivate,
        ]),
    ];
}

using Atlas.Identity.Domain.Entities.Tenants;

namespace Atlas.Identity.Application.Tenants.Repositories;

public interface ITenantRepository
{
    /// <summary>
    /// Loads tenant + Roles + Permissions by primary key.
    /// Use for role management commands (CreateRole, UpdateRole) that don't touch users or invitations.
    /// </summary>
    Task<Tenant?> GetByIdWithRolesAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Loads tenant + Roles (no Permissions) + Users + active Invitations by primary key.
    /// Active = not used AND not expired (i.ExpiresAt > UtcNow).
    /// Use for InviteUser: only needs to detect duplicate active invitations and verify the role exists.
    /// </summary>
    Task<Tenant?> GetByIdWithUsersActiveInvitationsAndRolesAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Loads tenant + Roles (no Permissions) + Users + ALL Invitations by primary key.
    /// No invitation filter — RemoveRole needs the full history to decide between
    /// hard-delete (no references ever) and soft-delete (historical references exist).
    /// </summary>
    Task<Tenant?> GetByIdWithUsersAllInvitationsAndRolesAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Loads tenant + Roles + Permissions + Users + Invitations by name.
    /// Used exclusively by ResolveTenantAccess, which runs during OIDC login before the
    /// tenant context is established — the tenant name comes from the token, not from session.
    /// </summary>
    Task<Tenant?> GetByNameWithUsersInvitationsAndRolesAsync(string name, CancellationToken ct);
}

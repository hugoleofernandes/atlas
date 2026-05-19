using Atlas.Identity.Domain.Entities.Tenants;

namespace Atlas.Identity.Application.Tenants.Repositories;

public interface ITenantRepository
{
    /// <summary>
    /// Loads tenant + Roles + Permissions only.
    /// Use for role management commands (Create, Update) that don't touch users or invitations.
    /// </summary>
    Task<Tenant?> GetByNameWithRolesAsync(string name, CancellationToken ct);

    /// <summary>
    /// Loads tenant + Roles + Permissions + Users + Invitations (non-used only).
    /// Invitations with IsUsed = true are excluded — they are already represented by their
    /// corresponding User in the Users collection, so domain invariants remain intact.
    /// Use when domain operations access all three collections (RemoveRole, InviteUser, ResolveAccess).
    /// </summary>
    Task<Tenant?> GetByNameWithUsersInvitationsAndRolesAsync(string name, CancellationToken ct);
}

using Atlas.Identity.Domain.Tenants;

namespace Atlas.Identity.Application.Aggregates.Tenants;

public interface ITenantRepository
{
    /// <summary>
    /// Loads tenant + Roles + Permissions by primary key.
    /// Use for role management commands (CreateRole, UpdateRole, RemoveRole).
    /// </summary>
    Task<Tenant?> GetByIdWithRolesAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Loads tenant + Roles + Permissions by name.
    /// Used exclusively by ResolveTenantAccess, which runs during OIDC login before the
    /// tenant context is established — the tenant name comes from the token, not from session.
    /// </summary>
    Task<Tenant?> GetByNameWithRolesAsync(string name, CancellationToken ct);
}

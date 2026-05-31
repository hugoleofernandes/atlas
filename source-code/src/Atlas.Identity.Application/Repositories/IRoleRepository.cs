using Atlas.Identity.Domain.Tenants._Roles;

namespace Atlas.Identity.Application.Repositories;

public interface IRoleRepository
{
    /// <summary>Loads a role with its permissions by ID. Returns null if not found.</summary>
    Task<Role?> GetByIdWithPermissionsAsync(Guid roleId, CancellationToken ct);

    /// <summary>Loads all roles (active and inactive) with permissions for a tenant.</summary>
    Task<IReadOnlyList<Role>> GetByTenantIdWithPermissionsAsync(Guid tenantId, CancellationToken ct);

    /// <summary>Returns true if any active role with the given name already exists for the tenant.</summary>
    Task<bool> ExistsWithNameAsync(Guid tenantId, string name, CancellationToken ct);

    /// <summary>Returns true if any role other than excludeRoleId has the given name for the tenant.</summary>
    Task<bool> ExistsWithNameExcludingAsync(Guid tenantId, string name, Guid excludeRoleId, CancellationToken ct);

    Task AddAsync(Role role, CancellationToken ct);

    void Remove(Role role);
}

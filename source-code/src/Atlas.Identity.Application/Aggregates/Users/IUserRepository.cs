using Atlas.Identity.Domain.Shared;
using Atlas.Identity.Domain.Users;

namespace Atlas.Identity.Application.Aggregates.Users;

public interface IUserRepository
{
    /// <summary>
    /// Returns the active user with the given email in the tenant, or null if not found.
    /// </summary>
    Task<User?> FindActiveByEmailAsync(Guid tenantId, Email email, CancellationToken ct);

    /// <summary>
    /// Returns true if any user (active or inactive) exists with the given email in the tenant.
    /// Used as a pre-check before creating an invitation.
    /// </summary>
    Task<bool> ExistsWithEmailAsync(Guid tenantId, Email email, CancellationToken ct);

    /// <summary>
    /// Returns true if any active user is assigned to the given role.
    /// Used by RemoveRole to check active blockers.
    /// </summary>
    Task<bool> HasActiveWithRoleAsync(Guid tenantId, Guid roleId, CancellationToken ct);

    /// <summary>
    /// Returns true if any user (active or inactive) was ever assigned to the given role.
    /// Used by RemoveRole to decide between hard-delete and soft-delete.
    /// </summary>
    Task<bool> HasAnyWithRoleAsync(Guid tenantId, Guid roleId, CancellationToken ct);

    Task AddAsync(User user, CancellationToken ct);
}

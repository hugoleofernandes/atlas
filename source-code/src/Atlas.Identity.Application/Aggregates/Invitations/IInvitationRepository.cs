using Atlas.Identity.Domain.Invitations;
using Atlas.Identity.Domain.Shared;

namespace Atlas.Identity.Application.Aggregates.Invitations;

public interface IInvitationRepository
{
    /// <summary>
    /// Returns the invitation for the given email in the tenant (any status), or null if not found.
    /// Used by ResolveTenantAccess to find the invitation to consume.
    /// </summary>
    Task<Invitation?> FindByEmailAsync(Guid tenantId, Email email, CancellationToken ct);

    /// <summary>
    /// Returns true if an active (not used and not expired) invitation exists for the given email.
    /// Used as a pre-check before creating a new invitation.
    /// </summary>
    Task<bool> HasActiveForEmailAsync(Guid tenantId, Email email, CancellationToken ct);

    /// <summary>
    /// Returns true if any active invitation is assigned to the given role.
    /// Used by RemoveRole to check active blockers.
    /// </summary>
    Task<bool> HasActiveWithRoleAsync(Guid tenantId, Guid roleId, CancellationToken ct);

    /// <summary>
    /// Returns true if any invitation (any status) was ever assigned to the given role.
    /// Used by RemoveRole to decide between hard-delete and soft-delete.
    /// </summary>
    Task<bool> HasAnyWithRoleAsync(Guid tenantId, Guid roleId, CancellationToken ct);

    Task AddAsync(Invitation invitation, CancellationToken ct);
}

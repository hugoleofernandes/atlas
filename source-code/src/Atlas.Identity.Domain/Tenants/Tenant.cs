using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Tenants;

/// <summary>
/// Represents an organization that defines an isolated business boundary within the system.
///
/// A tenant groups users under a shared context, controlling access,
/// roles, and membership within that scope.
///
/// Aggregate Root:
/// - Defines the boundary for user access and membership management.
///
/// Invariants:
/// - A tenant cannot have multiple active memberships for the same email.
/// - Memberships are always scoped to this tenant.
/// - Only active tenants allow membership operations.
///
/// Design Decisions:
/// - Supports invitation-first flow (email before user binding).
/// - Identity binding occurs after external authentication.
/// - Slug is a normalized identifier used for lookup and routing.
///
/// Boundaries:
/// - Does not handle authentication (external OIDC).
/// - Does not manage user lifecycle outside tenant context.
/// </summary>
public sealed class Tenant : IAggregateRoot
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Unique, URL-safe identifier used to locate the tenant (e.g., subdomain or route).
    /// </summary>
    public string Slug { get; private set; }

    public bool IsActive { get; private set; } = true;

    private readonly List<Membership> _memberships = new();
    public IReadOnlyCollection<Membership> Memberships => _memberships;

    private Tenant() { }

    public Tenant(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug is required.");

        Slug = slug.ToLowerInvariant();
    }

    public void EnsureActive()
    {
        if (!IsActive)
            throw new InvalidOperationException("Tenant is inactive.");
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
    }

    /// <summary>
    /// Invites a user to the tenant using email.
    ///
    /// Invariants:
    /// - Email must be unique among active memberships
    /// - Tenant must be active
    ///
    /// Throws:
    /// - InvalidOperationException when email is already invited
    /// - InvalidOperationException when tenant is inactive
    /// </summary>
    public void InviteUser(string email, string role)
    {
        EnsureActive();

        email = email.ToLowerInvariant();

        if (_memberships.Any(x => x.Email == email && x.IsActive))
            throw new InvalidOperationException("Email already invited.");

        _memberships.Add(new Membership(Id, email, role));
    }

    /// <summary>
    /// Returns the active membership for a user within the tenant.
    ///
    /// Invariants:
    /// - Tenant must be active
    /// - User must be linked to the tenant
    ///
    /// Throws:
    /// - InvalidOperationException when user is not linked
    /// </summary>
    public Membership GetActiveMembershipByUserId(Guid userId)
    {
        EnsureActive();

        var membership = _memberships
            .FirstOrDefault(x => x.UserId == userId && x.IsActive);

        if (membership is null)
            throw new InvalidOperationException("User not linked to this tenant.");

        return membership;
    }

    /// <summary>
    /// Resolves a membership for the given user.
    ///
    /// If the user is already linked, returns the existing membership.
    /// Otherwise, attempts to bind the user using an invited email.
    ///
    /// Invariants:
    /// - Tenant must be active
    /// - Email must correspond to an existing invitation
    ///
    /// Throws:
    /// - InvalidOperationException when no invitation exists
    /// </summary>
    public Membership BindUserToMembershipByEmail(Guid userId, string email)
    {
        EnsureActive();

        email = email.ToLowerInvariant();

        var membershipByEmail = _memberships.FirstOrDefault(x => x.Email == email && x.IsActive) ??
                                throw new InvalidOperationException("User not invited to this tenant.");

        membershipByEmail.BindUser(userId);
        return membershipByEmail;
    }

    public void DeactivateMembership(Guid userId)
    {
        EnsureActive();

        var membership = GetActiveMembershipByUserId(userId);
        membership.Deactivate();
    }
}
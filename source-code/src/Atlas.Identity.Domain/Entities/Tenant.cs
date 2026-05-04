using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities;

/// <summary>
/// Represents a tenant within the Identity domain.
///
/// Aggregate Root:
/// - Controls access to memberships and user invitations.
/// - Defines the boundary for user access within a tenant.
///
/// Invariants:
/// - A tenant cannot have multiple active memberships for the same email.
/// - Memberships are always associated with this tenant.
/// - Email uniqueness is enforced among active memberships.
///
/// Design Decisions:
/// - Users are invited via email before being linked to an IdentityUser.
/// - IdentityUser binding is optional and occurs after authentication.
/// - Slug is normalized to lowercase for consistency and lookup safety.
///
/// Boundaries:
/// - Does not manage authentication (handled externally via OIDC).
/// - Does not manage user lifecycle beyond tenant membership context.
/// </summary>
public sealed class Tenant : IAggregateRoot
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Slug { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<TenantMembership> _memberships = new();
    public IReadOnlyCollection<TenantMembership> Memberships => _memberships;

    private Tenant() { }

    public Tenant(string slug)
    {
        Slug = slug.ToLowerInvariant();
    }

    // 🔹 Convite / autorização por email
    public void InviteUser(string email, string role)
    {
        email = email.ToLowerInvariant();

        if (_memberships.Any(x => x.Email == email && x.IsActive))
            throw new InvalidOperationException("Email already invited.");

        _memberships.Add(new TenantMembership(Id, email, role));
    }

    public TenantMembership? FindMembershipByEmail(string email)
        => _memberships.FirstOrDefault(x =>
            x.Email == email.ToLowerInvariant() && x.IsActive);

    public TenantMembership? FindMembershipByUser(Guid userId)
        => _memberships.FirstOrDefault(x =>
            x.IdentityUserId == userId && x.IsActive);
}
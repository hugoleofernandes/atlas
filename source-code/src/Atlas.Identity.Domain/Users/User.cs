using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Users;

/// <summary>
/// Represents an external authenticated user identity provided by an identity provider (OIDC)..
///
/// Aggregate Root:
/// - Represents an authenticated user from an external identity provider.
///
/// Invariants:
/// - ExternalId uniquely identifies the user in the identity provider.
/// - A user can be deactivated but not deleted.
///
/// Design Decisions:
/// - Authentication is delegated to external providers (OIDC).
/// - The system does not manage passwords or credentials.
/// - IdentityUser is intentionally minimal and decoupled from domain-specific data.
///
/// Boundaries:
/// - Does not manage tenant membership (handled by Tenant aggregate).
/// - Does not store profile or business-related data.
/// </summary>
public sealed class User : IAggregateRoot
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string ExternalId { get; private set; }

    public User() { }

    public User(string externalId)
    {
        if (string.IsNullOrWhiteSpace(externalId))
            throw new ArgumentException("ExternalId is required.");

        ExternalId = externalId;
    }
}
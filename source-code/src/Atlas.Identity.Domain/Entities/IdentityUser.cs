using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities;

/// <summary>
/// Represents a user identity within the system.
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
public sealed class IdentityUser : IAggregateRoot
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string? ExternalId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public IdentityUser() { }

    public IdentityUser(string externalId)
    {
        ExternalId = externalId;
    }

    public void Deactivate() => IsActive = false;
}
using Atlas.Identity.Domain.Entities.Tenants.Events;
using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using Atlas.Identity.Domain.ValueObjects;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants;

/// <summary>
/// Purpose:
/// Represents an organizational boundary that owns users and invitations.
/// Controls access resolution and invitation lifecycle.
///
/// Invariants:
/// - A tenant cannot be inactive when performing domain operations.
/// - A tenant cannot have two users with the same email.
/// - A tenant cannot have two active invitations for the same email.
/// - A user must always be created from a valid and active invitation.
///
/// Boundaries:
/// - Does NOT validate external identity providers.
/// - Does NOT send emails or notifications.
/// - Does NOT persist data (handled by repositories/UoW).
///
/// Design Decisions:
/// - Users and Invitations belong to the Tenant because their lifecycle
///   and invariants depend on the tenant boundary.
/// - Access resolution is part of the Tenant because it enforces invariants
///   related to user creation and invitation usage.
/// </summary>
public sealed class Tenant : AggregateRootBase
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Name of the Microsoft Entra ID (Azure AD) tenant associated with this tenant.
    /// This value comes from the authentication context and identifies the
    /// Entra ID directory (e.g., "tenant01" or "tenant01.onmicrosoft.com").
    /// Not intended to be a user-friendly display name.
    /// </summary>
    public string Name { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private readonly List<User> _users = new();
    public IReadOnlyCollection<User> Users => _users;

    private readonly List<Invitation> _invitations = new();
    public IReadOnlyCollection<Invitation> Invitations => _invitations;

    private Tenant() { }

    /// <summary>
    /// Creates a new tenant.
    ///
    /// Invariants:
    /// - Name must be provided and normalized.
    /// </summary>
    public Tenant(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new TenantNameRequiredException();

        Name = name.ToLowerInvariant();
    }

    /// <summary>
    /// Ensures the tenant is active before performing domain operations.
    /// </summary>
    private void EnsureActive()
    {
        if (!IsActive)
            throw new TenantInactiveException();
    }

    /// <summary>
    /// Deactivates the tenant.
    ///
    /// Emits:
    /// - TenantDeactivatedDomainEvent
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        AddDomainEvent(new TenantDeactivatedDomainEvent(Id));
    }

    // =========================
    // DOMAIN BEHAVIOR
    // =========================

    /// <summary>
    /// Creates an invitation for a user within the tenant.
    ///
    /// Invariants:
    /// - Tenant must be active.
    /// - No two active invitations may exist for the same email.
    /// - Invitations cannot be created for existing users.
    ///
    /// Emits:
    /// - UserInvitedDomainEvent
    ///
    /// Throws:
    /// - TenantInactiveException
    /// - UserAlreadyExistsException
    /// - DuplicateInvitationException
    /// </summary>
    public Invitation InviteUser(Email email, Role role, InvitationTtl ttl)
    {
        EnsureActive();

        if (_users.Any(x => x.Email.Value == email.Value))
            throw new UserAlreadyExistsException(email.Value);

        var activeInvitation = _invitations
            .FirstOrDefault(x => x.Email.Value == email.Value && x.IsActive);

        if (activeInvitation is not null)
            throw new DuplicateInvitationException(email.Value);

        var invitation = new Invitation(Id, email, role, ttl);

        _invitations.Add(invitation);

        AddDomainEvent(new UserInvitedDomainEvent(Id, email.Value, role.Value));

        return invitation;
    }

    /// <summary>
    /// Resolves access for a user within the tenant.
    ///
    /// Behavior:
    /// - Returns an existing active user.
    /// - Or creates a new user from a valid invitation.
    ///
    /// Invariants:
    /// - Tenant must be active.
    /// - A user must come from a valid and active invitation.
    /// - No two users may share the same email.
    ///
    /// Emits:
    /// - InvitationUsedDomainEvent
    /// - UserCreatedFromInvitationDomainEvent
    /// - UserAccessResolvedDomainEvent
    ///
    /// Throws:
    /// - TenantInactiveException
    /// - InvitationNotFoundException
    /// - InvitationExpiredException
    /// - UserAlreadyExistsException
    /// </summary>
    public User ResolveAccess(ExternalId externalId, Email email)
    {
        EnsureActive();

        var existingUser = _users.FirstOrDefault(x => x.Email.Value == email.Value && x.IsActive);
        if (existingUser is not null)
        {
            if (existingUser.ExternalId.Value != externalId.Value)
                throw new UserAlreadyExistsException(email.Value);

            AddDomainEvent(new UserAccessResolvedDomainEvent(Id, existingUser.Id));
            return existingUser;
        }

        var invitation = _invitations.FirstOrDefault(x => x.Email.Value == email.Value)
            ?? throw new InvitationNotFoundException(email.Value);

        if (!invitation.IsActive)
            throw new InvitationExpiredException(email.Value);

        invitation.Use();
        AddDomainEvent(new InvitationUsedDomainEvent(Id, invitation.Id, invitation.Email.Value));

        var user = CreateUserFromInvitation(invitation, externalId);

        AddDomainEvent(new UserCreatedFromInvitationDomainEvent(
            Id, user.Id, user.Email.Value, user.Role.Value));

        AddDomainEvent(new UserAccessResolvedDomainEvent(Id, user.Id));

        return user;
    }

    /// <summary>
    /// Creates a new user from a valid invitation.
    /// </summary>
    private User CreateUserFromInvitation(Invitation invitation, ExternalId externalId)
    {
        var user = new User(Id, externalId, invitation.Email, invitation.Role);
        _users.Add(user);
        return user;
    }
}

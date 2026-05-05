using Atlas.Identity.Domain.Tenants.Events;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Tenants;

public sealed class Tenant : BaseEntity, IAggregateRoot
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Slug { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private readonly List<User> _users = new();
    public IReadOnlyCollection<User> Users => _users;

    private readonly List<Invitation> _invitations = new();
    public IReadOnlyCollection<Invitation> Invitations => _invitations;

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

    public void Deactivate() => IsActive = false;

    // =========================
    // DOMAIN BEHAVIOR
    // =========================

    public Invitation InviteUser(string email, string role, TimeSpan ttl)
    {
        EnsureActive();

        email = email.ToLowerInvariant();

        if (_invitations.Any(x => x.Email == email && !x.IsUsed))
            throw new InvalidOperationException("User already invited.");

        var invitation = new Invitation(Id, email, role, ttl);

        _invitations.Add(invitation);

        AddDomainEvent(new UserInvitedDomainEvent(Id, email, role));

        return invitation;
    }

    public User ResolveAccess(string externalId, string email)
    {
        EnsureActive();

        email = email.ToLowerInvariant();

        var existingUser = _users.FirstOrDefault(x => x.Email == email && x.IsActive);
        if (existingUser is not null)
        {
            AddDomainEvent(new UserAccessResolvedDomainEvent(Id, existingUser.Id));
            return existingUser;
        }

        var invitation = _invitations.FirstOrDefault(x => x.Email == email)
            ?? throw new InvalidOperationException("User not invited.");

        invitation.Use();
        AddDomainEvent(new InvitationUsedDomainEvent(Id, invitation.Id, invitation.Email));

        var user = new User(Id, externalId, email, invitation.Role);

        _users.Add(user);

        AddDomainEvent(new UserCreatedFromInvitationDomainEvent(
            Id, user.Id, user.Email, user.Role));

        AddDomainEvent(new UserAccessResolvedDomainEvent(Id, user.Id));

        return user;
    }
}
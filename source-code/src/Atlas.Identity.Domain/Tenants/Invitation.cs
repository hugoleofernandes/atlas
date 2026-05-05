using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Tenants;

public sealed class Invitation : IEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid TenantId { get; private set; }

    public string Email { get; private set; }

    public string Role { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; private set; }

    public bool IsUsed { get; private set; }

    public bool IsValid => !IsUsed && DateTime.UtcNow <= ExpiresAt;

    private Invitation() { }

    internal Invitation(Guid tenantId, string email, string role, TimeSpan ttl)
    {
        TenantId = tenantId;
        Email = email.ToLowerInvariant();
        Role = role;
        ExpiresAt = DateTime.UtcNow.Add(ttl);
    }

    public void Use()
    {
        if (IsUsed)
            throw new InvalidOperationException("Already used.");

        if (DateTime.UtcNow > ExpiresAt)
            throw new InvalidOperationException("Expired.");

        IsUsed = true;
    }
}
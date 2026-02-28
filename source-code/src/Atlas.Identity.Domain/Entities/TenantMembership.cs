namespace Atlas.Identity.Domain.Entities;

public sealed class TenantMembership
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid TenantId { get; private set; }

    public Guid? IdentityUserId { get; private set; }  // 🔹 AGORA NULLABLE

    public string Email { get; private set; }

    public string Role { get; private set; }

    public bool IsActive { get; private set; } = true;

    private TenantMembership() { }

    internal TenantMembership(Guid tenantId, string email, string role)
    {
        TenantId = tenantId;
        Email = email.ToLowerInvariant();
        Role = role;
    }

    public void BindIdentityUser(Guid identityUserId)
    {
        if (IdentityUserId.HasValue)
            return;

        IdentityUserId = identityUserId;
    }

    public void Deactivate() => IsActive = false;
}
namespace Atlas.Domain.Identity;

public sealed class TenantUser
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = default!;

    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;

    public string Email { get; private set; } = default!;

    public string Role { get; private set; } = "User";

    public bool IsActive { get; private set; } = true;

    private TenantUser() { }

    public TenantUser(Guid tenantId, Guid userId, string email, string role = "User")
    {
        TenantId = tenantId;
        UserId = userId;
        Email = email;
        Role = role;
    }

    public void Deactivate() => IsActive = false;
}
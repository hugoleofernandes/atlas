namespace Atlas.Domain.Identity;

public sealed class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    // OID do Microsoft Entra (único globalmente)
    public string? ExternalId { get; private set; }

    public bool IsActive { get; private set; } = true;

    private readonly List<TenantUser> _tenantUsers = new();
    public IReadOnlyCollection<TenantUser> TenantUsers => _tenantUsers;

    private User() { } // EF

    public User(string? externalId = null)
    {
        ExternalId = externalId;
    }

    public void SetExternalId(string oid)
    {
        if (!string.IsNullOrWhiteSpace(ExternalId))
            return;

        ExternalId = oid;
    }

    public void Deactivate() => IsActive = false;
}
namespace Atlas.Domain.Identity;

public sealed class Tenant
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Slug { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private readonly List<TenantUser> _tenantUsers = new();
    public IReadOnlyCollection<TenantUser> TenantUsers => _tenantUsers;

    private Tenant() { }

    public Tenant(string slug)
    {
        Slug = slug;
    }
}
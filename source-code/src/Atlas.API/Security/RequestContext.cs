using Atlas.SharedKernel.Application;

namespace Atlas.API.Security;

public sealed class RequestContext : IRequestContext
{
    private Guid? _tenantId;
    private string? _tenantSlug;
    private Guid? _userId;

    public bool IsAuthenticated => _tenantId.HasValue;

    public Guid? TenantId => _tenantId;
    public string? TenantSlug => _tenantSlug;
    public Guid? UserId => _userId;

    public void Set(Guid tenantId, string slug, Guid userId)
    {
        _tenantId = tenantId;
        _tenantSlug = slug;
        _userId = userId;
    }
}
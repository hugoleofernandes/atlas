using Atlas.SharedKernel.Application;

namespace Atlas.API.Security;

public sealed class RequestContext : IRequestContext, IRequestContextSetter
{
    private Guid? _tenantId;
    private string? _tenantName;
    private Guid? _userId;

    public bool IsAuthenticated => _tenantId.HasValue;

    public Guid? TenantId => _tenantId;
    public string? TenantName => _tenantName;
    public Guid? UserId => _userId;

    public void Set(Guid tenantId, string name, Guid userId)
    {
        _tenantId = tenantId;
        _tenantName = name;
        _userId = userId;
    }
}
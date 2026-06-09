using Atlas.SharedKernel.Application;

namespace Atlas.BuildingBlocks.AspNetCore.Security;

public sealed class RequestContext : IRequestContext, IRequestContextSetter
{
    private Guid?   _tenantId;
    private string? _tenantName;
    private Guid?   _userId;
    private string? _userEmail;
    private string? _correlationId;
    private bool    _tenantFilterSuspended;

    public bool     IsAuthenticated        => _tenantId.HasValue;
    public Guid?    TenantId               => _tenantId;
    public string?  TenantName             => _tenantName;
    public Guid?    UserId                 => _userId;
    public string?  UserEmail              => _userEmail;
    public string?  CorrelationId          => _correlationId;
    public bool     TenantFilterSuspended  => _tenantFilterSuspended;

    public void Set(Guid tenantId, string name, Guid userId, string? userEmail)
    {
        _tenantId   = tenantId;
        _tenantName = name;
        _userId     = userId;
        _userEmail  = userEmail;
    }

    public void SetCorrelationId(string correlationId)
        => _correlationId = correlationId;

    public IDisposable SuspendTenantFilter()
    {
        _tenantFilterSuspended = true;
        return new TenantFilterScope(() => _tenantFilterSuspended = false);
    }

    private sealed class TenantFilterScope(Action onDispose) : IDisposable
    {
        public void Dispose() => onDispose();
    }
}

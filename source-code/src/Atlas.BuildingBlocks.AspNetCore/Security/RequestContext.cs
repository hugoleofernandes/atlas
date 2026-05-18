using Atlas.SharedKernel.Application;

namespace Atlas.BuildingBlocks.AspNetCore.Security;

public sealed class RequestContext : IRequestContext, IRequestContextSetter
{
    private Guid? _tenantId;
    private string? _tenantName;
    private Guid? _userId;
    private string? _userEmail;

    public bool IsAuthenticated => _tenantId.HasValue;

    public Guid? TenantId => _tenantId;
    public string? TenantName => _tenantName;
    public Guid? UserId => _userId;
    public string? UserEmail => _userEmail;

    public void Set(Guid tenantId, string name, Guid userId, string? userEmail)
    {
        _tenantId = tenantId;
        _tenantName = name;
        _userId = userId;
        _userEmail = userEmail;
    }
}

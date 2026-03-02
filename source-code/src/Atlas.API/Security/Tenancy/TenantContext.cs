using Atlas.Identity.Application.Common;
using Atlas.SharedKernel.Application;

namespace Atlas.API.Security.Tenancy;

public sealed class TenantContext : ITenantContext, ITenantProvider
{
    private Guid? _tenantId;
    private string? _tenantSlug;

    public Guid TenantId =>
        _tenantId ?? throw new InvalidOperationException("Tenant was not resolved for this request.");

    public string TenantSlug =>
        _tenantSlug ?? throw new InvalidOperationException("Tenant was not resolved for this request.");

    public void Set(Guid tenantId, string tenantSlug)
    {
        if (_tenantId is not null)
            throw new InvalidOperationException("TenantContext already set for this request.");

        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(tenantSlug))
            throw new ArgumentException("TenantSlug cannot be null or empty.", nameof(tenantSlug));

        var normalized = tenantSlug.Trim().ToLowerInvariant();

        if (normalized.Length is < 2 or > 100)
            throw new ArgumentException("TenantSlug length is invalid.", nameof(tenantSlug));

        _tenantId = tenantId;
        _tenantSlug = normalized;
    }
}
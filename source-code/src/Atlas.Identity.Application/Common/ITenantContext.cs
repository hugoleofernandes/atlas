namespace Atlas.Identity.Application.Common;

public interface ITenantContext
{
    Guid TenantId { get; }

    string TenantSlug { get; }

    void Set(Guid tenantId, string tenantSlug);
}

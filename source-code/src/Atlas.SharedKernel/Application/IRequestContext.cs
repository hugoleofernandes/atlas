namespace Atlas.SharedKernel.Application;
public interface IRequestContext
{
    bool IsAuthenticated { get; }
    Guid? TenantId { get; }
    string? TenantSlug { get; }
    Guid? UserId { get; }

    void Set(Guid tenantId, string slug, Guid userId);
}
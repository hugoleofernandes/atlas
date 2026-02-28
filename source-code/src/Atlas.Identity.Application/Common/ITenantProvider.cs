namespace Atlas.Identity.Application.Common;

public interface ITenantProvider
{
    Guid TenantId { get; }
}

namespace Atlas.Application.Tenancy;

public interface ITenantProvider
{
    Guid TenantId { get; }
}

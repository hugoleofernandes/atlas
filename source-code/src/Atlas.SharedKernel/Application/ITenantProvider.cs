namespace Atlas.SharedKernel.Application;

public interface ITenantProvider
{
    Guid TenantId { get; }
}

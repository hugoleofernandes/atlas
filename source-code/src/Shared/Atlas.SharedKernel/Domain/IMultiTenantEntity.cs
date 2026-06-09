namespace Atlas.SharedKernel.Domain;

public interface IMultiTenantEntity
{
    Guid TenantId { get; }

    void SetTenantId(Guid tenantId);
}
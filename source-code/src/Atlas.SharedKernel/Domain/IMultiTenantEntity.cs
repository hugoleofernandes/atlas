namespace Atlas.SharedKernel.Domain;

public interface IMultiTenantEntity
{
    Guid TenantId { get; set; }
}
namespace Atlas.Domain.Common;

public interface IMultiTenantEntity
{
    Guid TenantId { get; set; }
}
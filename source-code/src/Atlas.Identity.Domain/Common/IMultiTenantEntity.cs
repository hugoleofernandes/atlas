namespace Atlas.Identity.Domain.Common;

public interface IMultiTenantEntity
{
    Guid TenantId { get; set; }
}
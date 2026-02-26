using Atlas.Domain.Common;

public class TestEntity : IMultiTenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = default!;
}
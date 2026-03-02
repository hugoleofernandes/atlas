namespace Atlas.BuildingBlocks.Audit;

public interface ICurrentTenant
{
    string? TenantId { get; }
}
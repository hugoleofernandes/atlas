namespace Atlas.Platform.Application.Queries.Tenants.GetTenantsByIds;

public interface IGetTenantsByIdsReader
{
    Task<IReadOnlyList<TenantLookupDto>> ReadAsync(IReadOnlyCollection<Guid> tenantIds, CancellationToken ct);
}

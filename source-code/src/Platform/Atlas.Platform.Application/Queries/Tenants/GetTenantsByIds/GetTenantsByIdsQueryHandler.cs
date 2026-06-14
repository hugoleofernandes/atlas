namespace Atlas.Platform.Application.Queries.Tenants.GetTenantsByIds;

public sealed class GetTenantsByIdsQueryHandler(IGetTenantsByIdsReader reader) : IGetTenantsByIdsQueryHandler
{
    public Task<IReadOnlyList<TenantLookupDto>> ExecuteAsync(GetTenantsByIdsQuery query, CancellationToken ct)
    {
        if (query.TenantIds.Count == 0)
            return Task.FromResult<IReadOnlyList<TenantLookupDto>>([]);

        return reader.ReadAsync(query.TenantIds, ct);
    }
}

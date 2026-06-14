namespace Atlas.Platform.Application.Queries.Tenants.GetTenantsByIds;

public sealed record GetTenantsByIdsQuery(IReadOnlyCollection<Guid> TenantIds);

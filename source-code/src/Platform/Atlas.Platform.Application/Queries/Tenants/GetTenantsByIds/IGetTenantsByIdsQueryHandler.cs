using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Platform.Application.Queries.Tenants.GetTenantsByIds;

public interface IGetTenantsByIdsQueryHandler
    : IQueryHandler<GetTenantsByIdsQuery, IReadOnlyList<TenantLookupDto>>;

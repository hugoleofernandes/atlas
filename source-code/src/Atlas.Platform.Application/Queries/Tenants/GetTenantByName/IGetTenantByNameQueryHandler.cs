using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Platform.Application.Queries.Tenants.GetTenantByName;

public interface IGetTenantByNameQueryHandler
    : IQueryHandler<GetTenantByNameQuery, TenantInfoDto>;

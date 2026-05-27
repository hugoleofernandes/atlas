using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Tenants.Queries.LookupRoles;

public interface ILookupRolesQueryHandler : IQueryHandler<LookupRolesQuery, IReadOnlyList<RoleLookupDto>>
{
}

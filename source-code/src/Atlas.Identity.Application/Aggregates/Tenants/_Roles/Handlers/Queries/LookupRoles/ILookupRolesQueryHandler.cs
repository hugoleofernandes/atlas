using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Queries.LookupRoles;

public interface ILookupRolesQueryHandler : IQueryHandler<LookupRolesQuery, IReadOnlyList<RoleLookupDto>>
{
}

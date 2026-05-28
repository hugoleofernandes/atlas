using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Tenants.Roles.Handlers.Queries.LookupRoles;

public interface ILookupRolesQueryHandler : IQueryHandler<LookupRolesQuery, IReadOnlyList<RoleLookupDto>>
{
}

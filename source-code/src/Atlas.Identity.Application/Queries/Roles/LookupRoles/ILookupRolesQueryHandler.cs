using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Queries.Roles.LookupRoles;

public interface ILookupRolesQueryHandler : IQueryHandler<LookupRolesQuery, IReadOnlyList<RoleLookupDto>>
{
}

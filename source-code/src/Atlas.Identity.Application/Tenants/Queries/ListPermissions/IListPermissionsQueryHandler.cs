using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Tenants.Queries.ListPermissions;

public interface IListPermissionsQueryHandler : IQueryHandler<ListPermissionsQuery, IReadOnlyList<PermissionGroupDto>>
{
}

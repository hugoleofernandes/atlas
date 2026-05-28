using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Aggregates.Tenants._Roles._Permissions.Handlers.Queries.ListPermissions;

public interface IListPermissionsQueryHandler : IQueryHandler<ListPermissionsQuery, IReadOnlyList<PermissionGroupDto>>
{
}

using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Tenants.Roles.Permissions.Handlers.Queries.ListPermissions;

public interface IListPermissionsQueryHandler : IQueryHandler<ListPermissionsQuery, IReadOnlyList<PermissionGroupDto>>
{
}

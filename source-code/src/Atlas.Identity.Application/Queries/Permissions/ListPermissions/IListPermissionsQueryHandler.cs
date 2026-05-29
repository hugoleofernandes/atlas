using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Queries.Permissions.ListPermissions;

public interface IListPermissionsQueryHandler : IQueryHandler<ListPermissionsQuery, IReadOnlyList<PermissionGroupDto>>
{
}

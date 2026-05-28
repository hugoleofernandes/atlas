namespace Atlas.Identity.Application.Aggregates.Tenants._Roles._Permissions.Handlers.Queries.ListPermissions;

public interface IListPermissionsReader
{
    IReadOnlyList<PermissionGroupDto> List();
}

namespace Atlas.Identity.Application.Tenants.Roles.Permissions.Handlers.Queries.ListPermissions;

public interface IListPermissionsReader
{
    IReadOnlyList<PermissionGroupDto> List();
}

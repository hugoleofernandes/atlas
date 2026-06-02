namespace Atlas.Identity.Application.Queries.Permissions.ListPermissions;

public interface IListPermissionsReader
{
    IReadOnlyList<PermissionModuleDto> List();
}

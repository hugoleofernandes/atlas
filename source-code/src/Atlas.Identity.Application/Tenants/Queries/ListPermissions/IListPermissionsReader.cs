using Atlas.Identity.Application.Tenants.Queries.Dtos;

namespace Atlas.Identity.Application.Tenants.Queries.ListPermissions;

public interface IListPermissionsReader
{
    IReadOnlyList<PermissionGroupDto> List();
}

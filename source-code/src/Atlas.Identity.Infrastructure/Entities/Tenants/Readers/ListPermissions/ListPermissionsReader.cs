using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.Identity.Application.Tenants.Queries.ListPermissions;
using Atlas.Identity.Domain.Entities.Tenants.Roles.Permissions;

namespace Atlas.Identity.Infrastructure.Entities.Tenants.Readers.ListPermissions;

/// <summary>
/// In-memory reader — wraps PermissionCatalog.Groups.
/// No database call; the permission catalog is a static domain constant.
/// </summary>
public sealed class ListPermissionsReader : IListPermissionsReader
{
    public IReadOnlyList<PermissionGroupDto> List()
    {
        return PermissionCatalog.Groups
            .Select(g => new PermissionGroupDto(g.Manage, g.Granular))
            .ToList();
    }
}

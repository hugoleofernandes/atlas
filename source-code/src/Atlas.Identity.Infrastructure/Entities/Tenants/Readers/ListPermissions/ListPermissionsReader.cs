using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.Identity.Application.Tenants.Queries.ListPermissions;
using Atlas.SharedKernel.Domain.Permissions;

namespace Atlas.Identity.Infrastructure.Entities.Tenants.Readers.ListPermissions;

/// <summary>
/// In-memory reader — wraps IPermissionPolicy.Groups.
/// No database call; the permission catalog is built at startup from registered modules.
/// </summary>
public sealed class ListPermissionsReader : IListPermissionsReader
{
    private readonly IPermissionPolicy _policy;

    public ListPermissionsReader(IPermissionPolicy policy)
    {
        _policy = policy;
    }

    public IReadOnlyList<PermissionGroupDto> List()
    {
        return _policy.Groups
            .Select(g => new PermissionGroupDto(g.Manage, g.Granular))
            .ToList();
    }
}

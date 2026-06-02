using Atlas.Identity.Application.Queries.Permissions.ListPermissions;
using Atlas.SharedKernel.Domain.Permissions;

namespace Atlas.Identity.Infrastructure.Readers.Permissions.ListPermissions;

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

    public IReadOnlyList<PermissionModuleDto> List()
    {
        return _policy.Modules
            .Select(module => new PermissionModuleDto(
                module.ModuleId,
                module.ModuleName,
                module.Groups.Select(g => new PermissionGroupDto(g.Manage, g.Granular)).ToList()))
            .ToList();
    }
}

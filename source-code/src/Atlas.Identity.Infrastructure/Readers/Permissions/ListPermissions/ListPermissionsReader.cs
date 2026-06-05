using Atlas.Identity.Application.Queries.Permissions.ListPermissions;
using Atlas.BuildingBlocks.Permissions;

namespace Atlas.Identity.Infrastructure.Readers.Permissions.ListPermissions;

/// <summary>
/// In-memory reader â€” wraps IPermissionPolicy.Groups.
/// No database call; the permission catalog is built at startup from registered modules.
/// </summary>
public sealed class ListPermissionsReader : IListPermissionsReader
{
    private readonly IPermissionPolicy _policy;

    public ListPermissionsReader(IPermissionPolicy policy)
    {
        _policy = policy;
    }

    public IReadOnlyList<PermissionItemDto> List()
    {
        return _policy
            .Modules.SelectMany(module =>
                module.Permissions.Select(code => new PermissionItemDto(module.ModuleId, module.ModuleName, code))
            )
            .ToList();
    }
}

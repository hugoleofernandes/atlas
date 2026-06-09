using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Application.Queries.Permissions.ListPermissions;

namespace Atlas.Identity.Infrastructure.Readers.Permissions.ListPermissions;

/// <summary>
/// Serves the BFF list-permissions endpoint from the in-memory cache.
/// No direct DB access — IPermissionCatalogCache handles loading and invalidation.
/// </summary>
public sealed class ListPermissionsReader(IPermissionCatalogCache cache) : IListPermissionsReader
{
    public async Task<IReadOnlyList<PermissionItemDto>> ListAsync(CancellationToken ct)
    {
        var all = await cache.GetAllActiveAsync(ct);
        return all
            .Where(p => !p.IsRoot)
            .Select(p => new PermissionItemDto(p.ModuleId!.Value, p.ModuleName!, p.Code))
            .ToList();
    }
}

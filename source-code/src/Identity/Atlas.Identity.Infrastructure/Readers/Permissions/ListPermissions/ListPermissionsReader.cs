using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Application.Queries.Permissions.ListPermissions;

namespace Atlas.Identity.Infrastructure.Readers.Permissions.ListPermissions;

/// <summary>
/// Serves the permissions list endpoint from the in-memory permission catalog cache.
/// Filtering is owned by the query handler.
/// </summary>
public sealed class ListPermissionsReader(IPermissionCatalogCache cache) : IListPermissionsReader
{
    public async Task<IReadOnlyList<PermissionItemDto>> ListAsync(CancellationToken ct)
    {
        var all = await cache.GetAllAsync(ct);

        return all
            .Where(p => !p.IsRoot && p.ModuleId is not null && p.ModuleName is not null)
            .Select(p => new PermissionItemDto(
                ModuleId: p.ModuleId!.Value,
                ModuleName: p.ModuleName!,
                Code: p.Code,
                Group: p.Group,
                IsActive: p.IsActive))
            .OrderBy(x => x.ModuleName)
            .ThenBy(x => x.Group)
            .ThenBy(x => x.Code)
            .ToList();
    }
}

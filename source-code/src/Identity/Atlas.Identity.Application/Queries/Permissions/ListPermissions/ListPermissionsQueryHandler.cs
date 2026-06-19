namespace Atlas.Identity.Application.Queries.Permissions.ListPermissions;

/// <summary>
/// Returns the permission catalog grouped by resource.
/// No tenant scope — the catalog is global and static.
/// </summary>
public sealed class ListPermissionsQueryHandler : IListPermissionsQueryHandler
{
    private readonly IListPermissionsReader _reader;

    public ListPermissionsQueryHandler(IListPermissionsReader reader)
    {
        _reader = reader;
    }

    public async Task<IReadOnlyList<PermissionItemDto>> ExecuteAsync(ListPermissionsQuery query, CancellationToken ct)
    {
        var all = await _reader.ListAsync(ct);

        if (query.IsActive is null)
            return all;

        return all.Where(x => x.IsActive == query.IsActive.Value).ToList();
    }
}

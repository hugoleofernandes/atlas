namespace Atlas.Identity.Application.Aggregates.Tenants._Roles._Permissions.Handlers.Queries.ListPermissions;

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

    public Task<IReadOnlyList<PermissionGroupDto>> ExecuteAsync(ListPermissionsQuery query, CancellationToken ct)
    {
        IReadOnlyList<PermissionGroupDto> result = _reader.List();
        return Task.FromResult(result);
    }
}

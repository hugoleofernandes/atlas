namespace Atlas.Identity.Application.Queries.Permissions.ListPermissions;

public interface IListPermissionsReader
{
    Task<IReadOnlyList<PermissionItemDto>> ListAsync(CancellationToken ct);
}

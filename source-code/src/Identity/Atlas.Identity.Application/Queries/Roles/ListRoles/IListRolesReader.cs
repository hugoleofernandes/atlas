namespace Atlas.Identity.Application.Queries.Roles.ListRoles;

public interface IListRolesReader
{
    Task<IReadOnlyList<ListRolesDto>> ListAsync(Guid tenantId, bool? isActive, CancellationToken ct);
}

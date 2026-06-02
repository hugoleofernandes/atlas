namespace Atlas.Identity.Application.Queries.Roles.ListRoles;

public interface IListRolesReader
{
    Task<IReadOnlyList<RoleDto>> ListAsync(Guid tenantId, bool? isActive, CancellationToken ct);
}

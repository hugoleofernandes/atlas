namespace Atlas.Identity.Application.Queries.Roles.LookupRoles;

public interface ILookupRolesReader
{
    Task<IReadOnlyList<RoleLookupDto>> LookupAsync(Guid tenantId, CancellationToken ct);
}

namespace Atlas.Identity.Application.Tenants.Roles.Handlers.Queries.LookupRoles;

public interface ILookupRolesReader
{
    Task<IReadOnlyList<RoleLookupDto>> LookupAsync(Guid tenantId, CancellationToken ct);
}

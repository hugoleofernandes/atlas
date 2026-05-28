namespace Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Queries.LookupRoles;

public interface ILookupRolesReader
{
    Task<IReadOnlyList<RoleLookupDto>> LookupAsync(Guid tenantId, CancellationToken ct);
}

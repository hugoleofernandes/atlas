using Atlas.Identity.Application.Tenants.Queries.Dtos;

namespace Atlas.Identity.Application.Tenants.Queries.LookupRoles;

public interface ILookupRolesReader
{
    Task<IReadOnlyList<RoleLookupDto>> LookupAsync(Guid tenantId, CancellationToken ct);
}

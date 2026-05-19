using Atlas.Identity.Application.Tenants.Queries.Dtos;

namespace Atlas.Identity.Application.Tenants.Queries.GetRoleById;

public interface IGetRoleByIdQueryHandler
{
    Task<RoleDto?> ExecuteAsync(GetRoleByIdQuery query, CancellationToken ct);
}
